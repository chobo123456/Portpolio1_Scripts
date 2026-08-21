using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public struct DecoGaveInfo
{
    public IDeco deco;
    public bool isMul;
}

public class CharacterStatContainer : IDeco
{
    public IDeco player;
    public bool isReady = false;
    private int currentCharacterIndex = 0;
    private Dictionary<bool, List<IDeco>> decos;
    public CharacterData StatData {get; private set;}
    public event System.Action onValueChanged;

    private readonly PlayerDataBox box;

    public CharacterStatContainer(PlayerDataBox box)
    {
        this.box = box;

        player = this;
       
        box.mono.RunRoutine(OnLoadComplete());

        currentCharacterIndex = box.CharacterIndex;

        EventBus.Sub("On_LevelUpgrade", OnLevelUpgrade);
    }

    public void Initialize()
    {
        StatData = DataLoader.GetData<CharacterData>(DataType.Character, box.CharacterId);
        currentCharacterIndex = box.CharacterIndex;
    }

    IEnumerator OnLoadComplete()
    {
        yield return new WaitUntil(() => LoadStatus.IsReady);

        StatData = DataLoader.GetData<CharacterData>(DataType.Character, box.CharacterId);

        isReady = true;
    }

    //새로운 데코 얻을시 처리
    public void SetDecorator(DecoGaveInfo info)
    {
        if(decos == null) decos = new();
        if(!decos.ContainsKey(info.isMul)) decos[info.isMul] = new();
        
        bool isNew = !decos[info.isMul].Contains(info.deco);
        if(isNew) decos[info.isMul].Add(info.deco);

        FindUsedDeco_And_Remove();

        IDeco newDeco = this;

        DecoKey decoKey = new DecoKey { index = currentCharacterIndex, characterId = box.CharacterId };

        //합연산
        if(decos.TryGetValue(false, out var add_list))
        {
            for(int i = 0; i < add_list.Count; i++)
            {
                var setable = add_list[i] as IDecoSetAble;
                newDeco = setable.SetDeco(newDeco);
            }
        }
        
        //곱연산
        if(decos.TryGetValue(true, out var mul_list))
        {
            for(int i = 0; i < mul_list.Count; i++)
            {
                var setable = mul_list[i] as IDecoSetAble;
                newDeco = setable.SetDeco(newDeco);
            }
        }

        if(isNew)
        {
            var newSetable = info.deco as IDecoSetAble;
            EventBus.Invoke<DecoKey, DecoInfo>("SetDecoUI", decoKey, newSetable.GetDecoInfo());
        }

        player = newDeco;
    }

    //사용된 데코 정리
    private void FindUsedDeco_And_Remove()
    {
        if(decos.TryGetValue(false, out var add_list))
        {
            for(int i = 0; i < add_list.Count; i++)
            {
                var deco = add_list[i];
                if(deco is Decorator decorator)
                {
                    if(decorator.IsFinishUse())
                        add_list.RemoveAt(i);
                }
            }
        }

        if(decos.TryGetValue(true, out var mul_list))
        {
            for(int i = 0; i < mul_list.Count; i++)
            {
                var deco = mul_list[i];
                if(deco is Decorator decorator)
                {
                    if(decorator.IsFinishUse())
                        mul_list.RemoveAt(i);
                }
            }
        }
    }
    
    public void Disable() 
    {
        EventBus.UnSub("On_LevelUpgrade", OnLevelUpgrade);
    } 

    public void OnLevelUpgrade()
    {
        onValueChanged?.Invoke();
    }

    public float GetHP()
    {
        int currentLevel = EventBus.Invoke_Func<int, int>("GetCharacterLevel", box.CharacterId);
        
        return StatData != null ? StatData.levelStep.GetMaxHpUseLevel(currentLevel) : 100f;
    } 

    public float GetMoveSpeed()         => StatData != null ? StatData.moveSpeed : 4f;
    public float GetAttackDamage()      
    {
        int currentLevel = EventBus.Invoke_Func<int, int>("GetCharacterLevel", box.CharacterId);

        float baseAttackDamage = StatData != null ? StatData.levelStep.GetBaseAttackDamageUseLevel(currentLevel) : 1f;

        return box.weapon != null ? box.weapon.GetInfo().damage + baseAttackDamage : 10f;
    }
    public float GetWeaponLength()      => box.weapon != null ? box.weapon.GetInfo().length : 0.7f;
    
    public IDeco UpdateNext()           => player;
}