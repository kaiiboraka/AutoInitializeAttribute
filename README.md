# AutoInitialize.cs
### A helpful Unity Utility script that automagically gets components for serialized attributes. 

This utility script was created by kaiiboraka and code_addict under a CC0 license.
Free to use and modify and extend to your hearts' content.

This was designed to alleviate the headache of constant `NullReferenceException`s upon
forgetting to connect objects in the inspector, or having to litter classes
with dozens of `GetComponent<T>` calls in `void Awake()`.

Throw this attribute on any object reference you want to be automatically initialized.
You can customize the depth to which your component is searched for by constructing
the attribute with one of the `SearchDepth` enum values, e.g. `[AutoInitialize(Parent)]`.

It is fairly performant as it will only search while there is no value,
stopping the search upon finding a valid value or erroring out if it finds no results.
Tooltips are shown if you mouse over the label seen in the inspector.

**NOTE:** This feature only works on serializable attributes, so a given variable with this
attribute needs to either be `public` or `[SerializeField] private`.

