using UnityEngine;

public class EshaPattern
{
    private Node node;
    private Node[] patterns;
    public EshaPattern(EnemyEntityDataBox box)
    {
        Node backNode =  new BackOriginalPosition(box); // 제자리 위치이동 명령

        Node summonClone = new SummonCloneOnRecordPoint(box, 0f, 101); //클론 소환
        Node summonSwordClone = new EshaSummonSword(box, 0f, 102);
        Node cloneExecuteNode = new ExecuteInPlaceClone(box); // 클론 제자리 공격명령
        Node cloneExecuteNode2 = new ExecuteDashClone(box); //클론 대쉬 명령 
        Node cloneClearNode        = new RemoveClone(box);

        Node summonProjectile = new SummonProjectile(box, 0f, 10001);

        Node recodeNode = new RecordNode(box); //플레이어 좌표 저장
        Node recodeClearNode = new ClearRecordNode(box);
        Node turnNode = new TurnNode(box, true); // 회전

        Node inVisible = new VanishNode(box, 0f, true); // 투명으로 변경
        Node visible = new VanishNode(box, 0f, false); // 불투명으로 변경

        Node eshaTeleportNode = new TeleportNode(box); // 보스 랜덤위치 이동

        Node miniDelay = new WaitingNode(box, 0.3f); // 딜레이 0.3초 
        Node delay = new WaitingNode(box, 0.5f); // 딜레이 0.5초 
        Node moreDelay = new WaitingNode(box, 0.7f);
        Node breakTime = new WaitingNode(box, 2f); //딜레이 2초
        Node restTime = new WaitingNode(box, 4f); //딜레이 4초

        Node dashNode = new DashNode(box, 1.7f); //보스 대쉬
        Node dashIndicator = new IndicatorNode(box, 0f, 0.15f, 1000001); // 보스 대쉬 인디케이터

        Node attack1 = 
            new AnimationTriggerNode(
                box,
                EnemyAnimationType.Pattern1,
                "Pattern1",
                "EshaAttack1",
                true,
                3f);
        
        Node attack2 = 
            new AnimationTriggerNode(
                box,
                EnemyAnimationType.Pattern2,
                "Pattern2",
                "EshaAttack2",
                true,
                5f);
        
        Node attack3 = 
            new AnimationTriggerNode(
                box,
                EnemyAnimationType.Pattern3,
                "Pattern3",
                "EshaAttack3",
                true,
                5f);      

        Node attack4 = 
            new AnimationTriggerNode(
                box,
                EnemyAnimationType.Pattern4,
                "Pattern4",
                "EshaAttack4",
                true,
                0f);

        Node attack5 = 
            new AnimationTriggerNode(
                box,
                EnemyAnimationType.Pattern5,
                "Pattern5",
                "EshaAttack5",
                true,
                0f);

        Node attack6 = 
            new AnimationTriggerNode(
                box,
                EnemyAnimationType.Pattern6,
                "Pattern6",
                "EshaAttack6",
                true,
                0f);

        #region Pattern1

        
        #region Pattern1 Step1

        Node P1St1 = new Sequence
            (
                inVisible,
                miniDelay,
                eshaTeleportNode,
                turnNode,
                visible,
                attack5,
                miniDelay,
                
                inVisible,
                miniDelay,
                eshaTeleportNode,
                turnNode,
                visible,
                attack5,
                miniDelay,
                
                inVisible,
                miniDelay,
                eshaTeleportNode,
                turnNode,
                visible,
                attack5,
                miniDelay
            );

        #endregion

        #region Pattern1 Step2
    
        Node P1St2 = new Sequence
            (
                inVisible,
                eshaTeleportNode,
                turnNode,
                visible,
                dashIndicator,
                dashNode,
                attack6,
                delay
            );
        #endregion

        #region Pattern1 Step3 

        Node P1St3 = new Sequence
            (
                inVisible,
                eshaTeleportNode,
                turnNode,
                visible,
                dashIndicator,
                dashNode,
                attack3,
                inVisible,
                eshaTeleportNode,
                turnNode,
                visible,
                attack2,
                breakTime
            );

        #endregion

        
        #endregion

        #region Pattern2

        #region Pattern2 Step1
        

        Node P2St1 = new Sequence
            (
                cloneClearNode,
                inVisible,
                backNode,
                turnNode,
                visible,
                attack4,
                miniDelay,

                inVisible,
                summonSwordClone,
                moreDelay,
                summonSwordClone,
                moreDelay,
                summonSwordClone,
                delay,
                summonSwordClone,
                delay,
                summonSwordClone,
                miniDelay,
                summonSwordClone,
                miniDelay,
                summonSwordClone,
                miniDelay,
                summonSwordClone,
                delay
            );

        #endregion

        #region Pattern2 Step2
        

        Node P2St2 = new Sequence
            (
                recodeClearNode,
                recodeNode,
                delay,
                recodeNode,
                miniDelay,
                recodeNode,
                miniDelay,
                recodeNode,
                miniDelay,
                recodeNode,
                miniDelay,
                summonClone,
                cloneExecuteNode2,
                cloneClearNode,
                recodeClearNode,
                miniDelay,

                eshaTeleportNode,
                turnNode,
                visible,
                attack5,
                delay,

                inVisible,
                eshaTeleportNode,
                turnNode,
                visible,
                attack6,
                cloneExecuteNode2,
                cloneClearNode,
                delay,
                
                inVisible,
                eshaTeleportNode,
                turnNode,
                visible,
                attack5,
                delay,

                inVisible,
                summonProjectile,
                miniDelay,
                summonProjectile,
                miniDelay,
                summonProjectile,
                miniDelay,
                summonProjectile,
                miniDelay,
                summonProjectile,
                miniDelay,
                summonProjectile,
                miniDelay,
                summonProjectile,
                miniDelay,
                summonProjectile,
                miniDelay,
                summonProjectile,
                miniDelay,
                summonProjectile,
                delay
            );

        #endregion

        #region Pattern2 Step3
        

        Node P2St3 = new Sequence
            (
                eshaTeleportNode,
                turnNode,
                visible,
                attack1,
                delay,

                inVisible,
                summonProjectile,
                summonProjectile,
                summonProjectile,
                eshaTeleportNode,
                turnNode,
                visible,
                attack1,
                delay,

                inVisible,
                summonProjectile,
                summonProjectile,
                summonProjectile,
                eshaTeleportNode,
                turnNode,
                visible,
                attack1,
                cloneExecuteNode,
                breakTime
            );
        #endregion

        #endregion

        #region GroggyNode

        Node groggyCondition    = new GroggyCondition(box);
        Node groggyStart        = new AnimationTriggerNode(
            box, 
            EnemyAnimationType.Groggy, 
            "Groggy", 
            "GroggyStart", 
            true,
            0f);

        Node groggyEnd          = new AnimationTriggerNode(
            box, 
            EnemyAnimationType.GroggyEnd, 
            "GroggyEnd", 
            "GroggyEnd", 
            true,
            0f);

        Node GroggySequence = new Sequence(
            groggyCondition,
            recodeClearNode,
            cloneClearNode,
            visible,
            groggyStart,
            restTime,
            groggyEnd
        );

        #endregion

        Sequence PT1 = new Sequence(P1St1, P1St2, P1St3);
        Sequence PT2 = new Sequence(P2St1, P2St2, P2St3);

        //Node1
        Node Pt1phaseCondition = new PhaseCondition(box, 1);
        Node Pt1Sequence = new Sequence(Pt1phaseCondition, PT1);

        //Node2
        Node Pt2phaseCondition = new PhaseCondition(box, 2);
        Node Pt2Sequence = new Sequence(Pt2phaseCondition, PT2);

        Node patterns = new Selector(GroggySequence, Pt2Sequence, Pt1Sequence);

        node = patterns;
    }

    public Node GetNode() => node;
}
