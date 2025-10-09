using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Manages a simple inventory UI using Unity's UI Toolkit (UIDocument).
/// Displays a scrollable list of items (added via "Add" button with random names).
/// Supports selection of items (click to select) and deletion (via "Delete" button: selected or last item).
/// Rows are styled as flex rows with labels; selection is visual (via CSS class).
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [SerializeField] UIDocument uiDocument;  
    [SerializeField] string addButtonName = "addButton";     
    [SerializeField] string deleteButtonName = "deleteButton"; 
    [SerializeField] string itemsPanelName = "itemsPanel";   

    // Array of random item names for demo purposes (added on "Add" click)
    private readonly string[] _randomNames =
    {
        "Sword","Apple","Potion","Shield","Bow","Gem","Key","Map","Ring","Axe","Torch","Bread"
    };

    // Cached UI references for performance (queried once on Enable)
    Button _addBtn;       // "Add" button reference
    Button _deleteBtn;    // "Delete" button reference
    ScrollView _itemsPanel;  // ScrollView for the list of item rows

    VisualElement _selectedRow;  // Currently selected item row (for deletion/highlighting)


    private void Awake()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();  
    }


    private void OnEnable()
    {
        var root = uiDocument.rootVisualElement;  // Get the root of the UI hierarchy

        // Query UI elements by name 
        _addBtn = root.Q<Button>(addButtonName);
        _deleteBtn = root.Q<Button>(deleteButtonName);
        _itemsPanel = root.Q<ScrollView>(itemsPanelName);

        // Subscribe to button events
        _addBtn.clicked += OnAddClicked;     
        _deleteBtn.clicked += OnDeleteClicked;  
    }

    /// <summary>
    /// Cleans up event subscriptions when the component is disabled.
    /// Prevents memory leaks from dangling callbacks.
    /// </summary>
    private void OnDisable()
    {
        if (_addBtn != null) _addBtn.clicked -= OnAddClicked;
        if (_deleteBtn != null) _deleteBtn.clicked -= OnDeleteClicked;
    }

    /// <summary>
    /// Adds a new item row with a random name from _randomNames.
    /// </summary>
    private void OnAddClicked()
    {
        string name = _randomNames[Random.Range(0, _randomNames.Length)];  // Pick random name
        AddItemRow(name);  // Create and add the row to the panel
    }

    /// <summary>
    /// Event handler for the "Delete" button.
    /// Deletes the currently selected row if any; otherwise, removes the last row in the list.
    /// Clears selection after deletion.
    /// </summary>
    private void OnDeleteClicked()
    {
        if (_selectedRow != null)
        {
            _selectedRow.RemoveFromHierarchy();  
            _selectedRow = null;  
            return;
        }

        // Fallback: Delete the last row if no selection
        var content = _itemsPanel.contentContainer;  
        int count = content.childCount;
        if (count > 0)
        {
            content.ElementAt(count - 1).RemoveFromHierarchy();
        }
    }

    /// <summary>
    /// Creates and adds a new item row to the ScrollView.
    /// Styles the row as a flex container with a label.
    /// </summary>
    private void AddItemRow(string itemName)
    {
        var row = new VisualElement();
        row.AddToClassList("item-row");  // Add USS class for custom styling (e.g., background, borders)

        row.style.flexDirection = FlexDirection.Row;
        row.style.alignItems = Align.Center;          
        row.style.justifyContent = Justify.SpaceBetween;  
        row.style.paddingLeft = 6;
        row.style.paddingRight = 6;
        row.style.paddingTop = 3;
        row.style.paddingBottom = 3;
        row.style.marginBottom = 4;  // Space between rows

        // Create and add the item label
        var label = new Label(itemName);
        label.AddToClassList("item-label");  
        row.Add(label);

        // Register click event for row selection (instead of per-row delete)
        row.RegisterCallback<ClickEvent>(_ => SelectRow(row));  // Lambda: On click, select this row

        // Add the row to the ScrollView's content container
        _itemsPanel.contentContainer.Add(row);

        // Auto-select the newly added row
        SelectRow(row);
    }

    /// <summary>
    /// Handles row selection on click.
    /// Deselects the previous row (removes "selected" class) and selects the new one.
    /// </summary>
    private void SelectRow(VisualElement row)
    {
        if (_selectedRow != null)
            _selectedRow.RemoveFromClassList("selected");  

        _selectedRow = row;  
        _selectedRow.AddToClassList("selected");  
    }
}
