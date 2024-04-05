/* This utility script was created by kaiiboraka and code_addict under a CC0 license.
 * Free to use and modify and extend to your hearts' content.
 * See <see href="https://github.com/kaiiboraka/AutoInitializeAttribute/blob/master/README.md">README.md</see>  for further details.
 */

using System;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static Utility.AutoInitializeAttribute.SearchDepth;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Utility
{
    ///<summary>
    /// Attribute to auto-initialize SERIALIZED component properties within Unity's editor.
    /// Non-public fields must have the [SerializeField] attribute.
    /// See <see href="https://github.com/kaiiboraka/AutoInitializeAttribute/blob/master/README.md">HERE</see> for further details.
    ///</summary>
    [Conditional("UNITY_EDITOR")]
    public class AutoInitializeAttribute : PropertyAttribute
    {
        ///<summary>
        /// Enumeration defining search depths for auto-initialization.
        ///</summary>
        public enum SearchDepth
        {
            ERROR, // Error state
            Self, // Only the GameObject itself
            Parent, // GameObject's parent
            Siblings, // GameObject's siblings
            Prefab, // Prefab instance root
            Scene // Entire scene
        }

        ///<summary>
        /// Specifies the search depth for auto-initialization.
        ///</summary>
        public SearchDepth depth;

        internal readonly SearchDepth cachedDepth;
        private const string LABEL_COLOR = "#FAA";
        public string label => $"<color={LABEL_COLOR}>[AutoInitialize:{cachedDepth.ToString()}]</color>";
        public string tooltip => $"{label} will auto-magically assign this component within the hierarchy" +
                                 $" to the specified search depth.\n\n{depthLabel}: {depthDesc}";
        public string depthLabel => $"<color={(depth == ERROR ? "#F33" : "#DDA")}>[{depth.ToString().ToUpper()}] </color>";
        private string depthDesc => depth switch {
            ERROR => $"Could not find the property at the specified depth. Searching is halted.\nClick the {depthLabel}label to try again.",
            Self =>"Search this GameObject and its children.",
            Parent =>"Search this GameObject and its parent.",
            Siblings =>"Search this GameObject's parent and all the parent's children.",
            Prefab =>"Search this GameObject's root prefab and all its children.",
            Scene =>"Search the entire scene. Equivalent to FindObjectOfType.",
            _ => throw new ArgumentOutOfRangeException(nameof(depth), depth, null)
        };

        ///<summary>
        /// Constructor for AutoInitializeAttribute.
        ///</summary>
        ///<param name="depth">The search depth for auto-initialization.</param>
        public AutoInitializeAttribute(SearchDepth depth = Self) => (this.depth,cachedDepth) = (depth,depth);

        public object Search(SerializedProperty property)
        {
            Type type = property.GetUnderlyingType();
            var gameObject = property.serializedObject.targetObject.GameObject();
            Transform parent = gameObject.transform.parent;
            var searchResult = depth switch
            {
                ERROR => null,
                Self => CheckSelf(),
                Parent => CheckParent(),
                Siblings => CheckRelatives(),
                Prefab => CheckPrefab(),
                Scene => CheckScene(),
                _ => throw new ArgumentOutOfRangeException()
            };

            if (searchResult != null) Debug.Log($"Initialized {label} {property.name}.");
            else if (depth != ERROR)
            {
                depth = ERROR;
                string errorMessage =
                    $"<color=#F33>Could not </color>{label}<color=#F33> property </color>" +
                    $"<color=#FF0>{property.name}</color><color=#F33> of </color>" +
                    $"<color=#FF0>{gameObject}</color>";
                Debug.LogError(errorMessage);
            }

            return searchResult;

            object CheckSelf() => gameObject.GetComponentInChildren(type);
            object CheckParent() => CheckSelf() ?? gameObject.GetComponentInParent(type);
            object CheckRelatives() => CheckSelf() ?? parent.GetComponentInChildren(type);
            object CheckPrefab() => CheckRelatives() ?? (gameObject.IsPrefabInstance()
                ? PrefabUtility.GetOutermostPrefabInstanceRoot(gameObject).GetComponentInChildren(type)
                : null);
            object CheckScene() => Object.FindObjectOfType(type);
        }
    }

    ///<summary>
    /// Custom property drawer for AutoInitializeAttribute.
    ///</summary>
    [CustomPropertyDrawer(typeof(AutoInitializeAttribute))]
    internal sealed class AutoInitializeAttributeEditor : PropertyDrawer
    {
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            AutoInitializeAttribute attr = attribute as AutoInitializeAttribute;
            
            if (property.GetUnderlyingValue() == null)
            {
                object o = attr.Search(property);
                if (o != null) property.SetUnderlyingValue(o);
            }

            PrintProperty(position, property, label, attr);
        }

        private static void PrintProperty(Rect position, SerializedProperty property, GUIContent label,
                                          AutoInitializeAttribute attr)
        {
            GUIStyle format = new()
                              {
                                  richText = true,
                                  alignment = TextAnchor.MiddleLeft,
                                  fontStyle = FontStyle.Bold
                              };
            format.fontSize = (int)format.lineHeight * 2 / 3;
            GUIContent prefix = new(attr.depthLabel, attr.tooltip);
            EditorGUI.BeginProperty(position, label, property);
            // EditorGUI.PrefixLabel(position, prefix, format);
            Rect labelPos = new Rect(position);
            var textWidth = format.CalcSize(prefix).x;
            labelPos.xMin += textWidth;
            EditorGUI.LabelField(labelPos, label);
            // if (attr.depth == ERROR && GUILayout.Button(prefix, GetBtnStyle()))
            Rect btnPos = new Rect(position);
            btnPos.width = textWidth;
            if (GUI.Button(btnPos,prefix, format) && attr.depth == ERROR) attr.depth = attr.cachedDepth;

            EditorGUI.ObjectField(position, property, new GUIContent(" "));
            EditorGUI.EndProperty();
        }
    }
    
    
}
