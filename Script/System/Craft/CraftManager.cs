using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
public struct CraftInfo
{
    public int itemId;
    public int currentAmount;
    public int needAmount;
}

[System.Serializable]
public struct PlayerCraftRecipe
{
    public List<int> recipeIds;
}

public class PlayerCraftList
{
    private Save<PlayerCraftRecipe> playerCraftRecipe;
    private List<int> currentRecipeList = new();
    public PlayerCraftList()
    {
        playerCraftRecipe = new("Player/CraftRecipe", "CraftRecipeData");

        if(playerCraftRecipe.IsExist())
            Load();
        else
            AddRecipe(1);
    }

    public bool IsContainedRecipe(int recipeId) => currentRecipeList.Contains(recipeId);

    public int GetRecipe(int index) => currentRecipeList[index];
    public void AddRecipe(int recipeId)
    {
        currentRecipeList.Add(recipeId);
        Save();
    }

    private void Load()
    {
        currentRecipeList = playerCraftRecipe.savedData.recipeIds;
    }

    private void Save()
    {
        PlayerCraftRecipe playerCraftRecipeData = new();
        playerCraftRecipeData.recipeIds = currentRecipeList;

        playerCraftRecipe.Saving(playerCraftRecipeData);
    }

    public IReadOnlyList<int> GetPlayerCraftRecipeList() => currentRecipeList;
}

public class CraftItemReceiver
{
    public void Craft(IReadOnlyList<CraftInfo> info, int resultCraftItemId, int craftAmount)
    {
        for(int i = 0; i < info.Count; i++)
        {
            CraftInfo currentInfo = info[i];

            int removeItemId     = currentInfo.itemId;
            int removeItemAmount = currentInfo.needAmount;

            Util.Log($"remove Item ||  id | {removeItemId}  amount | -{removeItemAmount}");

            EventBus.Invoke<int, int, bool>("GetItem", removeItemId, -removeItemAmount, false);
        }

        Util.Log($"get Item ||  id | {resultCraftItemId}  amount |{craftAmount}");
        EventBus.Invoke<int, int, bool>("GetItem", resultCraftItemId, craftAmount, false);

        //퀘스트 
        EventBus.Invoke<QuestType, int>("QuestManager_OnAskQuestFinish", QuestType.Craft, resultCraftItemId);
    }
}

public class CraftCalculator
{
    private List<CraftInfo> currentOriginCraftInfo = new();
    private int currentCraftAmount = 1;
    public Dictionary<int, int> GetMaterialAmount(CraftRecipe recipeData)
    {
        Dictionary<int, ItemHasInfo> playerMaterials = 
            EventBus.Invoke_Func<InventoryType, Dictionary<int, ItemHasInfo>>("Inventory_System_GetInventory_Dic", InventoryType.Material);

        CraftRecipeMaterial[] craftMaterials = recipeData.recipe_material;

        Dictionary<int, int> itemList = new();

        for(int i = 0; i < craftMaterials.Length; i++)
        {
            int craftMaterialId = craftMaterials[i].recipe_material_Id;     

            if(playerMaterials.TryGetValue(craftMaterialId, out ItemHasInfo info))
                itemList.Add(craftMaterialId, info.itemAmount);
            else
                itemList.Add(craftMaterialId, 0);
        }

        return itemList;
    }

    public void SetOriginCraftInfo(List<CraftInfo> craftInfo)
    {
        currentOriginCraftInfo = craftInfo;
    }

    public int GetMaxCraftAbleAmount()
    {
        currentCraftAmount = 1;

        int maxCraftAbleAmount = 0;
        int addedValue = 0;

        for(int i = 0; i < currentOriginCraftInfo.Count; i++)
        {
            CraftInfo currentInfo = currentOriginCraftInfo[i];

            int currentAmount = currentInfo.currentAmount;
            int needAmount    = currentInfo.needAmount;

            int maxAmount = currentAmount / needAmount;
            addedValue += maxAmount;
        }

        maxCraftAbleAmount = addedValue / currentOriginCraftInfo.Count;

        return maxCraftAbleAmount;
    }

    public void SetCraftAmount(int multiple = 1)
    {
        currentCraftAmount = multiple;
    }

    public IReadOnlyList<CraftInfo> GetCurrentCraftInfo()
    {
        List<CraftInfo> newList = new();

        for(int i = 0; i < currentOriginCraftInfo.Count; i++)
        {
            CraftInfo info = currentOriginCraftInfo[i];
            info.needAmount *= currentCraftAmount;
            newList.Add(info);
        }

        return newList;
    } 

    public int GetCurrentCraftAmount() => currentCraftAmount;
}

public class CraftUI
{
    private GameObject recipe_button_prefab, recipe_material_icon;
    private readonly MonoBehaviour managerMono;
    private readonly CraftCalculator calculater;
    private readonly CraftItemReceiver receiver;
    private readonly int materialIconInitializeCount = 20;
    
    //UI
    public Slider craftAmountSlider;
    public Button craftButton;
    private TextMeshProUGUI craftAmountText;
    //IconList
    private List<Craft_Recipe_Button> recipe_button_List = new();
    private List<Craft_Material> material_Icon_List = new();

    //Flag
    private bool isReadyToCraft = false, initialized = false;
    private bool isLock_CraftButton = false, isLock_RecipeButton = false;
    private int currentRecipeId = 1;

    public CraftUI(MonoBehaviour managerMono, IReadOnlyList<int> readonlyList)
    {
        calculater = new();
        receiver   = new();

        this.managerMono = managerMono;
        
        managerMono.RunRoutine(Initialize(readonlyList));
    }

    public void OnEnable()
    {
        SubscribeEvent(true);
    }

    public void OnDisable()
    {
        SubscribeEvent(false);
    }

    private void SubscribeEvent(bool isSubscribe)
    {
        if(isSubscribe)
        {
            EventBus.Sub<bool>("Craft_UI_Lock_CraftButton", LockCraftButton);
            EventBus.Sub<bool>("Craft_UI_Lock_RecipeButton", LockRecipeButton);
        }
        else
        {
            EventBus.UnSub<bool>("Craft_UI_Lock_CraftButton", LockCraftButton);
            EventBus.UnSub<bool>("Craft_UI_Lock_RecipeButton", LockRecipeButton);
        }
    }
    //Init
    IEnumerator Initialize(IReadOnlyList<int> readonlyList)
    {
        Initialize_RecipeButtonList(readonlyList);
        Initialize_MaterialIconList();
        Initialize_Slider();
        Initialize_Text();
        Initialize_Button();
        
        yield return new WaitUntil(() => 
            recipe_button_prefab != null && 
            recipe_material_icon != null &&
            material_Icon_List.Count > 0);
        
        currentRecipeId = readonlyList[0];
        OnClick_Recipe(currentRecipeId);

        initialized = true;
    }

    private async void Initialize_RecipeButtonList(IReadOnlyList<int> readonlyList)
    {
        if(recipe_button_prefab == null)
            recipe_button_prefab = await AddressableUtil.Load_Instant<GameObject>("Craft_Recipe", managerMono.GetCancelOnDestroy());

        if(managerMono == null) return;
        Transform parentTr = managerMono.transform.FindTarget("Craft_Recipe_List");
        for(int i = 0; i < 20; i++)
        {
            GameObject newRecipeButton = GameObject.Instantiate(recipe_button_prefab);
            newRecipeButton.transform.SetParent(parentTr);
            
            Craft_Recipe_Button recipeComp = newRecipeButton.GetComponent<Craft_Recipe_Button>();
            recipe_button_List.Add(recipeComp);

            if(i < readonlyList.Count)
            {
                int index = i;
                int recipeId = readonlyList[index];
                
                if(recipeComp != null) 
                    recipeComp.Initialize(recipeId);

                Button recipeButton = newRecipeButton.GetComponent<Button>();
                if(recipeButton != null) 
                    recipeButton.onClick.AddListener(() => OnClick_Recipe(recipeId, true));

                newRecipeButton.SetActive(true);
            }
            else
            {
                newRecipeButton.SetActive(false);
            }
        }
    }

    private async void Initialize_MaterialIconList()
    {
        if(recipe_material_icon == null)
            recipe_material_icon = await AddressableUtil.Load_Instant<GameObject>("Craft_Material", managerMono.GetCancelOnDestroy());
        
        Transform parentTr = managerMono.transform.FindTarget("Craft_Material_List");
        for(int i = 0; i < materialIconInitializeCount; i++)
        {
            GameObject newMaterialIcon = GameObject.Instantiate(recipe_material_icon);
            newMaterialIcon.transform.SetParent(parentTr);

            Craft_Material materialComp = newMaterialIcon.GetComponent<Craft_Material>();
            if(materialComp != null) 
                materialComp.Initialize();

            material_Icon_List.Add(materialComp);

            newMaterialIcon.SetActive(false);
        }
    }

    private void Initialize_Slider()
    {
        craftAmountSlider = managerMono.transform.FindTarget("Craft_Amount_Slider").GetComponent<Slider>();
        craftAmountSlider.onValueChanged.AddListener(OnValueChanged_CraftAmount);
    }

    private void Initialize_Button()
    {
        craftButton = managerMono.transform.FindTarget("Craft_Button").GetComponentInChildren<Button>();
        craftButton.onClick.AddListener(OnClick_FinishCraftButton);
    }
    
    private void Initialize_Text()
    {
        craftAmountText = craftAmountSlider.transform.FindTarget("Craft_Amount_Text").GetComponent<TextMeshProUGUI>();        
    }
    
    public void ReloadUI(int startRecipeId = 0)
    {
        if(startRecipeId == 0) 
            startRecipeId = currentRecipeId;

        OnClick_Recipe(startRecipeId);
    }

    //Interact
    private void OnClick_Recipe(int recipeId, bool isButtonClicked = false)
    {
        if(isLock_RecipeButton) return;

        ClearMaterialIconsData();

        currentRecipeId = recipeId;

        isReadyToCraft = false;

        CraftRecipe recipeData = DataLoader.GetData<CraftRecipe>(DataType.Recipe, recipeId);
        
        Dictionary<int, int> itemAmount = calculater.GetMaterialAmount(recipeData);
        List<CraftInfo> currentCraftInfo = new();

        bool isCraftAble = true;

        for(int i = 0; i < material_Icon_List.Count; i++)
        {
            Craft_Material materialIcon = material_Icon_List[i];

            if(i < recipeData.recipe_material.Length)
            {
                CraftRecipeMaterial craftMaterial = recipeData.recipe_material[i];
                int craftMaterialId = craftMaterial.recipe_material_Id;  
                int needAmount      = craftMaterial.needAmount;  
                itemAmount.TryGetValue(craftMaterialId, out int currentAmount);

                materialIcon.SetData(craftMaterialId, needAmount, currentAmount);
                materialIcon.gameObject.SetActive(true);

                if(!materialIcon.IsAbleToMake()) 
                    isCraftAble = false;
                
                CraftInfo newInfo = new();
                newInfo.itemId          = craftMaterialId;
                newInfo.currentAmount   = currentAmount;
                newInfo.needAmount      = needAmount;

                currentCraftInfo.Add(newInfo);
            }
            else
                materialIcon.gameObject.SetActive(false);
        }

        if(isCraftAble)
        {
            calculater.SetOriginCraftInfo(currentCraftInfo);
            craftAmountSlider.minValue = 1;
            craftAmountSlider.maxValue = calculater.GetMaxCraftAbleAmount();
            craftAmountSlider.value = craftAmountSlider.minValue;

            if(craftAmountSlider.maxValue > 1)
                craftAmountSlider.gameObject.SetActive(true);
            else
                craftAmountSlider.gameObject.SetActive(false);

            isReadyToCraft = true;
        } 
        else
            craftAmountSlider.gameObject.SetActive(false);

        if(isButtonClicked) EventBus.Invoke<int>("On_Craft_UI_RecipeClick", recipeId);
    }
    
    private void OnValueChanged_CraftAmount(float amount)
    {
        int parseValue = (int)amount;

        craftAmountText.SetText($"{parseValue}");

        calculater.SetCraftAmount(parseValue);
        IReadOnlyList<CraftInfo> fixedList = calculater.GetCurrentCraftInfo();

        bool isCraftAble = true;

        for(int i = 0; i < material_Icon_List.Count; i++)
        {
            if(i < fixedList.Count)
            {
                Craft_Material materialIcon = material_Icon_List[i];

                if(materialIcon.IsSetData())
                {
                    if(!materialIcon.IsAbleToMake()) 
                        isCraftAble = false;

                    CraftInfo info = fixedList[i];
                    materialIcon.SetData(info.itemId, info.needAmount, info.currentAmount);
                }
            }
            else
                break;
        }

        if(!isCraftAble)
            isReadyToCraft = false;
    }

    private void OnClick_FinishCraftButton()
    {
        if(!isReadyToCraft || isLock_CraftButton) return;

        int craftItemAmount = calculater.GetCurrentCraftAmount();

        CraftRecipe recipeData = DataLoader.GetData<CraftRecipe>(DataType.Recipe, currentRecipeId);
        int itemId = recipeData.result_item_Id;

        receiver.Craft(calculater.GetCurrentCraftInfo(), itemId, craftItemAmount);

        ClearMaterialIconsData();

        if(GameState.IsTutorial())
            EventBus.Invoke("On_Craft_UI_CraftClick");
    }

    private void ClearMaterialIconsData()
    {
        for(int i = 0; i < material_Icon_List.Count; i++)
        {
            Craft_Material materialIcon = material_Icon_List[i];

            if(materialIcon.IsSetData())
                materialIcon.SetData(-999);
        }
    }

    //AddRecipe_Seq
    public void AddRecipe(int newRecipeId)
    {
        Craft_Recipe_Button newRecipeButton = GetUnInitializedRecipeButton();

        if(newRecipeButton != null) 
            newRecipeButton.Initialize(newRecipeId);

        Button recipeButton = newRecipeButton.GetComponent<Button>();
        if(recipeButton != null) 
            recipeButton.onClick.AddListener(() => OnClick_Recipe(newRecipeId));
    }

    private Craft_Recipe_Button GetUnInitializedRecipeButton()
    {
        for(int i = 0; i < recipe_button_List.Count; i++)
        {
            Craft_Recipe_Button recipeButton = recipe_button_List[i];
            if(!recipeButton.IsInitialized())   
                return recipeButton;
        }

        AddMoreList();

        return GetUnInitializedRecipeButton();
    }

    private void AddMoreList()
    {
        Transform parentTr = managerMono.transform.FindTarget("Craft_Recipe_List");

        for(int i = 0; i < 20; i++)
        {
            GameObject newRecipeButton = GameObject.Instantiate(recipe_button_prefab);
            newRecipeButton.transform.SetParent(parentTr);
            
            Craft_Recipe_Button recipeComp = newRecipeButton.GetComponent<Craft_Recipe_Button>();
            recipe_button_List.Add(recipeComp);
        }
    }

    //FlagProp
    public bool IsReady() => initialized;


    private void LockCraftButton(bool isLock)
    {
        isLock_CraftButton = isLock;
    }

    private void LockRecipeButton(bool isLock)
    {
        isLock_RecipeButton = isLock;
    }
}

public class CraftManager : UIClass
{
    private PlayerCraftList playerCraftRecipeList;
    private CraftUI craftUI;
    private GameObject mainPanel;

    private bool IsInit = false;
    private WaitUntil craftUiReady;
    public override void OnEnable()
    {
        base.SetType(UIType.Craft);
        base.OnEnable();

        this.RunRoutine(Initialize(), "CraftManager_Initialize");
    }

    IEnumerator Initialize()
    {
        Initialize_Event();
        Initialize_Object();

        yield return new WaitUntil(() => LoadStatus.IsReady && LoadStatus.IsReady_Inventory);

        playerCraftRecipeList = new();
        craftUI = new(this.GetComponent<MonoBehaviour>(), playerCraftRecipeList.GetPlayerCraftRecipeList());
        craftUI.OnEnable();

        craftUiReady = new WaitUntil(() => craftUI != null && craftUI.IsReady());

        yield return craftUiReady;

        LoadStatus.SetStatus(ManagerType.Craft, true);
        IsInit = true;
    }

    private void Initialize_Event()
    {
        EventBus.Sub<int>("GetRecipe", OnAddRecipe);
        EventBus.Sub("InventoryReload", OnInventoryReload);
    }
    private void Initialize_Object()
    {
        mainPanel = transform.FindTarget("mainPanel").gameObject;
        mainPanel.SetActive(false);

        Button craftPanelCloseButton = transform.FindTarget("Craft_Close_Button").GetComponent<Button>();
        craftPanelCloseButton.onClick.AddListener(() => base.OnClickCloseButton());
    }

    private void OnDisable()
    {
        craftUI?.OnDisable();

        LoadStatus.SetStatus(ManagerType.Craft, false);

        EventBus.UnSub<int>("GetRecipe", OnAddRecipe);
        EventBus.UnSub("InventoryReload", OnInventoryReload);
    }
    
    private void OnInventoryReload()
    {
        if(!IsInit) return;

        craftUI.ReloadUI();
    }

    private void OnAddRecipe(int newRecipeId)
    {
        if(playerCraftRecipeList.IsContainedRecipe(newRecipeId)) return;

        playerCraftRecipeList.AddRecipe(newRecipeId);
        craftUI.AddRecipe(newRecipeId);
    }

    public override bool IsReady()
    {
        return IsInit;
    }
    
    public override void Open()
    {
        craftUI.ReloadUI(playerCraftRecipeList.GetRecipe(0));
        mainPanel.SetActive(true);
    }
    public override void Close()
    {
        mainPanel.SetActive(false);
    }

    public override RectTransform GetRectTransform(UIRectName rectName) 
    { 
        switch(rectName)
        {
            case UIRectName.CraftUI_RecipeButton:
                return this.transform.FindTarget("RecipePanel").GetComponent<RectTransform>();

            case UIRectName.CraftUI_AmountSlider:
                return craftUI.craftAmountSlider.GetComponent<RectTransform>();

            case UIRectName.CraftUI_CraftButton:
                return craftUI.craftButton.GetComponent<RectTransform>();

            case UIRectName.CraftUI_MaterialList:
                return this.transform.FindTarget("ItemLine").GetComponent<RectTransform>();
        }

        return null;
    }
}
