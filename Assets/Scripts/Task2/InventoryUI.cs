using UnityEngine;
using UnityEngine.UIElements;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] UIDocument uiDocument;
    [SerializeField] string addButtonName = "addButton";
    [SerializeField] string deleteButtonName = "deleteButton";
    [SerializeField] string itemsPanelName = "itemsPanel";

    private readonly string[] _randomNames =
    {
        "Sword","Apple","Potion","Shield","Bow","Gem","Key","Map","Ring","Axe","Torch","Bread"
    };

    Button _addBtn;
    Button _deleteBtn;
    ScrollView _itemsPanel;

    VisualElement _selectedRow;

    private void Awake()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
    }

    private void OnEnable()
    {
        var root = uiDocument.rootVisualElement;

        _addBtn = root.Q<Button>(addButtonName);
        _deleteBtn = root.Q<Button>(deleteButtonName);
        _itemsPanel = root.Q<ScrollView>(itemsPanelName);

        _addBtn.clicked += OnAddClicked;
        _deleteBtn.clicked += OnDeleteClicked;
    }

    private void OnDisable()
    {
        if (_addBtn != null) _addBtn.clicked -= OnAddClicked;
        if (_deleteBtn != null) _deleteBtn.clicked -= OnDeleteClicked;
    }

    private void OnAddClicked()
    {
        string name = _randomNames[Random.Range(0, _randomNames.Length)];
        AddItemRow(name);
    }

    private void OnDeleteClicked()
    {
        if (_selectedRow != null)
        {
            _selectedRow.RemoveFromHierarchy();
            _selectedRow = null;
            return;
        }

        var content = _itemsPanel.contentContainer;
        int count = content.childCount;
        if (count > 0)
        {
            content.ElementAt(count - 1).RemoveFromHierarchy();
        }
    }

    private void AddItemRow(string itemName)
    {
        var row = new VisualElement();
        row.AddToClassList("item-row");
        row.style.flexDirection = FlexDirection.Row;
        row.style.alignItems = Align.Center;
        row.style.justifyContent = Justify.SpaceBetween;
        row.style.paddingLeft = 6;
        row.style.paddingRight = 6;
        row.style.paddingTop = 3;
        row.style.paddingBottom = 3;
        row.style.marginBottom = 4;

        var label = new Label(itemName);
        label.AddToClassList("item-label");
        row.Add(label);

       /* var xBtn = new Button(() =>
        {
            if (_selectedRow == row) _selectedRow = null;
            row.RemoveFromHierarchy();
        });
        xBtn.text = "X";
        xBtn.AddToClassList("item-delete");
        xBtn.style.width = 24;
        xBtn.style.height = 20;
        row.Add(xBtn);*/

        row.RegisterCallback<ClickEvent>(_ => SelectRow(row));

        _itemsPanel.contentContainer.Add(row);

        SelectRow(row);
    }

    private void SelectRow(VisualElement row)
    {
        if (_selectedRow != null)
            _selectedRow.RemoveFromClassList("selected");

        _selectedRow = row;
        _selectedRow.AddToClassList("selected");
    }
}
