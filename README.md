# AutoInitialize.cs
### A helpful Unity Utility script that automagically gets components for serialized attributes. 

This utility script was created by kaiiboraka and code_addict under a CC0 license.
Free to use and modify and extend to your hearts' content.

This was designed to alleviate the headache of constant `NullReferenceException`s upon forgetting to connect objects in the inspector, or having to litter classes with dozens of `GetComponent<T>` calls in `void Awake()`.

Throw this attribute on any object reference you want to be automatically initialized. You can customize the depth to which your component is searched for by constructing the attribute with one of the `SearchDepth` enum values, e.g. `[AutoInitialize(Parent)]`.

Tooltips are shown if you mouse over the label seen in the inspector.

**NOTE:** This feature only works on serializable attributes, so a given variable with this attribute needs to either be `public` or `[SerializeField] private`.

## **FAQ**

**Q: What are the advantages to using this over functionality that I'm used to?**

Cool Thing #1: save you some keystrokes, who doesn't love that? Oftentimes with code you'll end up with many calls to `GetComponent<T>` throughout a project, so this should eliminate 90% of those.
Cool Thing #2: \[AutoInitialize\] runs during the editor's OnGUI, so it'll populate fields before runtime and keep them that way, unlike `GetComponent` calls made in `Awake`.
Cool Thing #3: Set and forget! Even if left in an \[ERROR\] state from an unsuccessful search, the attribute will try to search again every time the Object's inspector changes (such as the addition or removal of components, not when field values are changed), on a domain reload, or simply if you re-select the given GameObject in the hierarchy (click off to a different one and back on). So if the components didn't exist at first when you made the variable with the attribute, but then you create them later in the spot it's expecting, you won't need to remember to go back and fill it in yourself.

**Q: Will this script slow down my editor performance?**

Not to any significant degree! The script runs in the editor's `OnGUI` method, so it is fairly performant as it will only search while the GameObject with the attribute is focused, and while the field for the property is empty/set to `None`. If it tries and is unable to locate the component with the given `SearchDepth` it will change to an `[ERROR]` state and indicate that it was unsuccessful. This label can be clicked to force a retry. 

**Q: Will this Attribute pick the wrong component, such as when there are multiple instances of it in the hierarchy?**

Ultimately the attribute is a sort of shorthand for the various versions of Unity's `GetComponent<T>`, so any points of failure would be the same as when those methods wouldn't work either. In other words, if the Attribute ends up grabbing the wrong component, it is then up to the developer and how they design their own object hierarchy. Any complex hierarchy navigation beyond at most something like "see if a sibling game object has it" would likely need manual finding or assignment. At the moment it is only designed to handle when there is a single known instance of a component within the provided `SearchDepth`.

That being said, if you have any suggestions for what type of additional navigation use case you'd like to see, we'd be more than happy to try and implement a solution for it!

**Q: I put the attribute on a variable, but it's still not working. What gives?**

This feature only works on serializable attributes, so a given variable with this attribute needs to either be `public` or `[SerializeField] private` or equivalent. In other words, double check to see that your variable isn't left being just `private`/`protected` without a corresponding `[SerializeField]` attribute. E.g.: `[AutoInitialize, SerializeField] private RigidBody2D x;`. Also be sure that the data type of the variable is a MonoBehavior derivative--in other words, it needs to be a component.
