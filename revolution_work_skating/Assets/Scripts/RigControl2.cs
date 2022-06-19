using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class RigControl2 : MonoBehaviour
{
    public const int SIZE = 50;
    public const int WALKING_SYCLE = 10;
    public const int JOINTS = 12;
    public const float CHANGE_SEC = 0.5f;
    public const float GENERATION_CYCLE = 20.0f;
    //各関節の限界角度(左右対称)
    public const int X_HEAD = 30; //-30~30

    public const int X_BODYSPINE = 80; //0~40

    public const int X_UPPER_ARM = 80; //-80~80

    public const int X_LOWER_ARM = 160; //0~160

    public const int Y_HAND = 90; //-90~90
    //この２つはセット
    public const int Z_UPPER_LEG_LEFT = -45; //-45~0
    public const int Z_UPPER_LEG_RIGHT = 45; //0~45
    public const int X_LOWER_LEG = 60; //0~60
    //関節角度の上限・下限
    public int[,] minMaxScale = new int[JOINTS,2]{
                                      {-1*X_HEAD, X_HEAD+1},
                                      {0, X_BODYSPINE+1},
                                      {-1*X_UPPER_ARM, X_UPPER_ARM+1},
                                      {0, X_LOWER_ARM+1},
                                      {-1*Y_HAND, Y_HAND+1},
                                      {Z_UPPER_LEG_LEFT, 0+1},
                                      {0, X_LOWER_LEG+1},
                                      {-1*X_UPPER_ARM, X_UPPER_ARM+1},
                                      {0, X_LOWER_ARM+1},
                                      {-1*Y_HAND, Y_HAND+1},
                                      {0, Z_UPPER_LEG_RIGHT+1},
                                      {0, X_LOWER_LEG+1}
                                      };
    //スキーヤー配列
    public GameObject[] skier;
    public Vector3 bodyRotation = new Vector3(0,0,0);
    //頭
    RigBone[] head;
    //胴体
    RigBone[] bodySpine;
    //左腕
    RigBone[] leftUpperArm;
    RigBone[] leftLowerArm;
    RigBone[] leftHand;
    //右腕
    RigBone[] rightUpperArm;
    RigBone[] rightLowerArm;
    RigBone[] rightHand;
    //左脚
    RigBone[] leftUpperLeg;
    RigBone[] leftLowerLeg;
    //右脚
    RigBone[] rightUpperLeg;
    RigBone[] rightLowerLeg;
    //遺伝子
    int[,,] genes;
    private float timeElapsed = 0;
    private float stateFlow = 0;
    private float stopstate = 0;
    private float firststate = 0;
    private int state;
    //現在の世代数
    public static int GENERATION = 0;
    float[] position = new float[SIZE];
    Vector3[] rotation = new Vector3[SIZE];
    Vector3[] skierPos = new Vector3[SIZE];
    public static float[] SCORE;
    public static int[] STAND;
    public static float MAX;
    public static float AVG_SCORE;
    public static string FLOW = "";
    public static string maxFLOW = "";
    List<int> numbers;

    void Start()
    {
        //頭
        head = new RigBone[SIZE];
        //胴体
        bodySpine = new RigBone[SIZE];
        //左腕
        leftUpperArm = new RigBone[SIZE];
        leftLowerArm = new RigBone[SIZE];
        leftHand = new RigBone[SIZE];
        //右腕
        rightUpperArm = new RigBone[SIZE];
        rightLowerArm = new RigBone[SIZE];
        rightHand = new RigBone[SIZE];
        //左脚
        leftUpperLeg = new RigBone[SIZE];
        leftLowerLeg = new RigBone[SIZE];
        //右脚
        rightUpperLeg = new RigBone[SIZE];
        rightLowerLeg = new RigBone[SIZE];

        state = 0;
        skier = new GameObject[SIZE];
        //複製
        makeInstance();
        //遺伝子生成
        genes = new int[SIZE, WALKING_SYCLE, JOINTS];
        SCORE = new float[SIZE];
        STAND = new int[SIZE];
        makeRandomGenes();
    }
    void Update()
    {
        //250世代まで実験
        if(GENERATION+1 > 250){
            Debug.Log(FLOW);
            Debug.Log(maxFLOW);
            Quit();
        }
        timeElapsed += Time.deltaTime;
        //20秒毎に世代を交代する
        if(timeElapsed > GENERATION_CYCLE) {
            //評価
            evaluate();
            //初期位置に戻す
            setStartPos();
            GENERATION++;
            //タイマーリセット
            timeElapsed = 0;
            stopstate = 0;
            firststate = 0;
            stateFlow = 0;
            SCORE = new float[SIZE];
            STAND = new int[SIZE];
        }
        //各世代3.0s後に行動を開始する
        stopstate += Time.deltaTime;
        if(stopstate > 3.0f) {
            firststate += Time.deltaTime;
            if(firststate <= CHANGE_SEC) {
                //初期状態からstate0の状態へ
                for(int k=0; k<SIZE; k++) {
                    //頭の動き
                    head[k].offset((float)(genes[k, state%WALKING_SYCLE, 0]*(firststate/CHANGE_SEC)), 1, 0, 0);
                    //胴体
                    bodySpine[k].offset((float)(genes[k, state%WALKING_SYCLE, 1]*(firststate/CHANGE_SEC)), 1, 0, 0);
                    //左腕(上)
                    leftUpperArm[k].offset((float)(genes[k, state%WALKING_SYCLE, 2]*(firststate/CHANGE_SEC)), 1, 0, 0);
                    //左腕(下)
                    leftLowerArm[k].offset((float)(genes[k, state%WALKING_SYCLE, 3]*(firststate/CHANGE_SEC)), 1, 0, 0);
                    //左手
                    leftHand[k].offset((float)(genes[k, state%WALKING_SYCLE, 4]*(firststate/CHANGE_SEC)), 0, 1, 0);
                    //左脚(上)
                    leftUpperLeg[k].offset((float)(genes[k, state%WALKING_SYCLE, 5]*(firststate/CHANGE_SEC)), 0, 1, 2);
                    //左脚(下)
                    leftLowerLeg[k].offset((float)(genes[k, state%WALKING_SYCLE, 6]*(firststate/CHANGE_SEC)), 1, 0, 0);
                    //右腕(上)
                    rightUpperArm[k].offset((float)(genes[k, state%WALKING_SYCLE, 7]*(firststate/CHANGE_SEC)), 1, 0, 0);
                    //右腕(下)
                    rightLowerArm[k].offset((float)(genes[k, state%WALKING_SYCLE, 8]*(firststate/CHANGE_SEC)), 1, 0, 0);
                    //右手
                    rightHand[k].offset((float)(genes[k, state%WALKING_SYCLE, 9]*(firststate/CHANGE_SEC)), 0, 1, 0);
                    //右脚(上)
                    rightUpperLeg[k].offset((float)(genes[k, state%WALKING_SYCLE, 10]*(firststate/CHANGE_SEC)), 0, 1, 2);
                    //右脚(下)
                    rightLowerLeg[k].offset((float)(genes[k, state%WALKING_SYCLE, 11]*(firststate/CHANGE_SEC)), 1, 0, 0);
                }
            }
            else {
                stateFlow += Time.deltaTime;
                for (int k=0; k<SIZE; k++){
                    if(skier[k].transform.position.z > position[k]){
                        SCORE[k] += skier[k].transform.position.z - position[k];
                    }
                    //位置取得
                    position[k] = skier[k].transform.position.z;
                    //回転角取得
                    rotation[k] = skier[k].transform.localEulerAngles;
                    if(STAND[k] == 0 && ((rotation[k].x >= 80 && rotation[k].x <= 280) || (rotation[k].y >= 80 && rotation[k].y <= 280) || (rotation[k].z >= 80 && rotation[k].z <= 280))){
                        STAND[k] = -1;
                    } else if(STAND[k] == 0) {
                    //state1からstate6を繰り返す
                    //頭の動き
                    head[k].offset((float)(genes[k, state%WALKING_SYCLE, 0] + (genes[k, (state+1)%WALKING_SYCLE, 0] - genes[k, state%WALKING_SYCLE, 0])*(stateFlow/CHANGE_SEC)), 1, 0, 0);
                    //胴体
                    bodySpine[k].set((float)(genes[k, state%WALKING_SYCLE, 1] + (genes[k, (state+1)%WALKING_SYCLE, 1] - genes[k, state%WALKING_SYCLE, 1])*(stateFlow/CHANGE_SEC)), 1, 0, 0);
                    //左腕(上)
                    leftUpperArm[k].offset((float)(genes[k, state%WALKING_SYCLE, 2] + (genes[k, (state+1)%WALKING_SYCLE, 2] - genes[k, state%WALKING_SYCLE, 2])*(stateFlow/CHANGE_SEC)), 1, 0, 0);
                    //左腕(下)
                    leftLowerArm[k].offset((float)(genes[k, state%WALKING_SYCLE, 3] + (genes[k, (state+1)%WALKING_SYCLE, 3] - genes[k, state%WALKING_SYCLE, 3])*(stateFlow/CHANGE_SEC)), 1, 0, 0);
                    //左手
                    leftHand[k].offset((float)(genes[k, state%WALKING_SYCLE, 4] + (genes[k, (state+1)%WALKING_SYCLE, 4] - genes[k, state%WALKING_SYCLE, 4])*(stateFlow/CHANGE_SEC)), 0, 1, 0);
                    //左脚(上)
                    leftUpperLeg[k].offset((float)(genes[k, state%WALKING_SYCLE, 5] + (genes[k, (state+1)%WALKING_SYCLE, 5] - genes[k, state%WALKING_SYCLE, 5])*(stateFlow/CHANGE_SEC)), 0, 1, 2);
                    //左脚(下)
                    leftLowerLeg[k].offset((float)(genes[k, state%WALKING_SYCLE, 6] + (genes[k, (state+1)%WALKING_SYCLE, 6] - genes[k, state%WALKING_SYCLE, 6])*(stateFlow/CHANGE_SEC)), 1, 0, 0);
                    //右腕(上)
                    rightUpperArm[k].offset((float)(genes[k, state%WALKING_SYCLE, 7] + (genes[k, (state+1)%WALKING_SYCLE, 7] - genes[k, state%WALKING_SYCLE, 7])*(stateFlow/CHANGE_SEC)), 1, 0, 0);
                    //右腕(下)
                    rightLowerArm[k].offset((float)(genes[k, state%WALKING_SYCLE, 8] + (genes[k, (state+1)%WALKING_SYCLE, 8] - genes[k, state%WALKING_SYCLE, 8])*(stateFlow/CHANGE_SEC)), 1, 0, 0);
                    //右手
                    rightHand[k].offset((float)(genes[k, state%WALKING_SYCLE, 9] + (genes[k, (state+1)%WALKING_SYCLE, 9] - genes[k, state%WALKING_SYCLE, 9])*(stateFlow/CHANGE_SEC)), 0, 1, 0);
                    //右脚(上)
                    rightUpperLeg[k].offset((float)(genes[k, state%WALKING_SYCLE, 10] + (genes[k, (state+1)%WALKING_SYCLE, 10] - genes[k, state%WALKING_SYCLE, 10])*(stateFlow/CHANGE_SEC)), 0, 1, 2);
                    //右脚(下)
                    rightLowerLeg[k].offset((float)(genes[k, state%WALKING_SYCLE, 11] + (genes[k, (state+1)%WALKING_SYCLE, 11] - genes[k, state%WALKING_SYCLE, 11])*(stateFlow/CHANGE_SEC)), 1, 0, 0);
                    }
                }
            }
        }
        //状態遷移
        if(stateFlow > CHANGE_SEC) {
            stateFlow = 0;
            state++;
        }
    }

    //オブジェクトのインスタンス化
    void makeInstance(){
        for (int i=0; i<SIZE; i++){
            if(i==0) skier[0] = GameObject.Find("skier(Clone)");
            else {
                skier[i] = Instantiate(skier[0]);
                skier[i].transform.Translate(30*i, 0, 0);
            }
            skierPos[i] = skier[i].transform.position;
            //頭インスタンス
            head[i] = new RigBone(skier[i], HumanBodyBones.Head);
            //胴体インスタンス化
            bodySpine[i] = new RigBone(skier[i], HumanBodyBones.Spine);
            //左腕インスタンス化
            leftUpperArm[i] = new RigBone(skier[i], HumanBodyBones.LeftUpperArm);
            leftLowerArm[i] = new RigBone(skier[i], HumanBodyBones.LeftLowerArm);
            leftHand[i] = new RigBone(skier[i], HumanBodyBones.LeftHand);
            //右腕インスタンス化
            rightUpperArm[i] = new RigBone(skier[i], HumanBodyBones.RightUpperArm);
            rightLowerArm[i] = new RigBone(skier[i], HumanBodyBones.RightLowerArm);
            rightHand[i] = new RigBone(skier[i], HumanBodyBones.RightHand);
            //左脚インスタンス化
            leftUpperLeg[i] = new RigBone(skier[i], HumanBodyBones.LeftUpperLeg);
            leftLowerLeg[i] = new RigBone(skier[i], HumanBodyBones.LeftLowerLeg);
            //右脚インスタンス化
            rightUpperLeg[i] = new RigBone(skier[i], HumanBodyBones.RightUpperLeg);
            rightLowerLeg[i] = new RigBone(skier[i], HumanBodyBones.RightLowerLeg);        
        }
    }

    //オブジェクトの削除
    void deleteObject() {
        for (int i=0; i<SIZE; i++) {
            Destroy(skier[i]);
        }
    }

    //初期位置をセット
    void setStartPos(){
        for(int i=0; i<SIZE; i++) {
                skier[i].transform.position = skierPos[i];
                //胴体
                bodySpine[i].set(0, 1, 0, 0);
                bodySpine[i].set(0, 0, 1, 0);
                bodySpine[i].set(0, 0, 0, 1);
                skier[i].transform.rotation 
                    = Quaternion.AngleAxis(bodyRotation.z,new Vector3(0,0,1))
                    * Quaternion.AngleAxis(bodyRotation.x,new Vector3(1,0,0))
                    * Quaternion.AngleAxis(bodyRotation.y,new Vector3(0,1,0));
                //頭の動き
                head[i].offset(0, 1, 0, 0);
                head[i].offset(0, 0, 1, 0);
                head[i].offset(0, 0, 0, 1);
                //左腕(上)
                leftUpperArm[i].offset(0, 1, 0, 0);
                leftUpperArm[i].offset(0, 0, 1, 0);
                //左腕(下)
                leftLowerArm[i].offset(0, 1, 0, 0);
                //左手
                leftHand[i].offset(0, 1, 0, 0);
                leftHand[i].offset(0, 0, 1, 0);
                leftHand[i].offset(0, 0, 0, 1);
                //左脚(上)
                leftUpperLeg[i].offset(0, 1, 0, 0);
                leftUpperLeg[i].offset(0, 0, 1, 0);
                leftUpperLeg[i].offset(0, 0, 0, 1);
                //左脚(下)
                leftLowerLeg[i].offset(0, 1, 0, 0);
                //右腕(上)
                rightUpperArm[i].offset(0, 1, 0, 0);
                rightUpperArm[i].offset(0, 0, 1, 0);
                //右腕(下)
                rightLowerArm[i].offset(0, 1, 0, 0);
                //右手
                rightHand[i].offset(0, 1, 0, 0);
                rightHand[i].offset(0, 0, 1, 0);
                rightHand[i].offset(0, 0, 0, 1);
                //右脚(上)
                rightUpperLeg[i].offset(0, 1, 0, 0);
                rightUpperLeg[i].offset(0, 0, 1, 0);
                rightUpperLeg[i].offset(0, 0, 0, 1);
                //右脚(下)
                rightLowerLeg[i].offset(0, 1, 0, 0);
        }
    }


    //初期遺伝子生成(ランダム)
    void makeRandomGenes(){
        System.Random r = new System.Random();
        for (int k=0; k<SIZE; k++){
            for(int i=0; i<WALKING_SYCLE; i++){
                //半周期ずれて左右対称
                if(i < WALKING_SYCLE/2){
                    for(int j=0; j<JOINTS; j++){
                        genes[k, i, j] = r.Next(minMaxScale[j, 0], minMaxScale[j, 1]);
                    }
                } else {
                    for(int j=0; j<JOINTS; j++){
                        //前半の右の動作を左の動作にコピー
                        if(j>=2 && j<=6){
                            //股の角度は反転
                            if(j==5) genes[k, i, j] = -1*genes[k, i-WALKING_SYCLE/2, j+5];
                            else genes[k, i, j] = genes[k, i-WALKING_SYCLE/2, j+5];
                        }
                        //前半の左の動作を右の動作にコピー
                        else if(j>=7){
                            //股の角度は反転
                            if(j==10) genes[k, i, j] = -1*genes[k, i-WALKING_SYCLE/2, j-5];
                            else genes[k, i, j] = genes[k, i-WALKING_SYCLE/2, j-5];
                        } else {
                            genes[k, i, j] = r.Next(minMaxScale[j, 0], minMaxScale[j, 1]);
                        }
                    } 
                }
            }
        }
    }

    //選択
    void evaluate() {
        AVG_SCORE = 0;
        int[] random = new int[4];
        int[] parent = new int[4];
        int[] index = new int[SCORE.Length];
        int temp=0, prob;
        for (int k = 0; k < SCORE.Length; k++) {
            //転倒しなかったスキーヤーの評価
            if(STAND[k] == 0) SCORE[k] += 5;
            //平均スコア計算
            AVG_SCORE += SCORE[k];
        }
        //スコアとインデックを降順ソート
        for (int i = 0; i < SCORE.Length; ++i) { index[i] = i; }
        Array.Sort(SCORE, index);
        Array.Reverse(SCORE);
        Array.Reverse(index);
        //最大スコア更新
        MAX = SCORE[0];
        //平均スコア更新
        AVG_SCORE /= SCORE.Length;
        FLOW += (AVG_SCORE).ToString() + ',';
        maxFLOW += (MAX).ToString() + ',';
        Debug.Log(MAX);
        Debug.Log(index[0]);
        Debug.Log(AVG_SCORE);
        //6点交叉
        System.Random r = new System.Random();
        //上位以外の遺伝子の交換
        for(int k=0; k<SIZE - 5; k++) {
            //上位5体から2体を選ぶ(インデックス)
            numbers = new List<int>();    
            for (int i = 0; i <= 4; i++) {
                numbers.Add(i);
            }
            while (numbers.Count > 3) {
                if(numbers.Count == 5){
                    prob = r.Next(0, 101);
                    if(prob < 5) temp = 4;
                    else if(prob < 15) temp = 3;
                    else if(prob < 30) temp = 2;
                    else if(prob < 60) temp = 1;
                    else temp = 0;
                }
                else if(numbers.Count == 4){
                    prob = r.Next(0, 101);
                    if(prob < 10) temp = 3;
                    else if(prob < 25) temp = 2;
                    else if(prob < 55) temp = 1;
                    else temp = 0;
                }
                parent[5 - numbers.Count] = index[numbers[temp]];
                numbers.RemoveAt(temp);
            }
            if(k==parent[0] || k==parent[1]) continue;
            for(int i=0; i<WALKING_SYCLE; i++) {
                //2値乱数生成
                for (int j = 0; j < random.Length; j++)
                {   
                    random[j] = r.Next(0, 2);
                }
                //半周期ずれて左右対称
                if(i < WALKING_SYCLE/2){
                    for(int j=0; j<JOINTS; j++) {
                        //頭
                        if(j==0) genes[k, i, j] = random[0] == 0 ? genes[parent[0], i, j] : genes[parent[1], i, j];
                        //胴体
                        else if(j==1) genes[k, i, j] = random[1] == 0 ? genes[parent[0], i, j] : genes[parent[1], i, j];
                        //左腕
                        else if(j<=4) genes[k, i, j] = random[2] == 0 ? genes[parent[0], i, j] : genes[parent[1], i, j];
                        //左脚
                        else if(j<=6) genes[k, i, j] = random[2] == 0 ? genes[parent[0], i, j] : genes[parent[1], i, j];
                        //右腕
                        else if(j<=9) genes[k, i, j] = random[3] == 0 ? genes[parent[0], i, j] : genes[parent[1], i, j];
                        //右脚
                        else genes[k, i, j] = random[3] == 0 ? genes[parent[0], i, j] : genes[parent[1], i, j];
                        //突然変異
                        if (r.Next(0, 100) < 2) genes[k, i, j] = r.Next(minMaxScale[j, 0], minMaxScale[j, 1]);
                    }
                } else {
                    for(int j=0; j<JOINTS; j++){
                        //前半の右の動作を左の動作にコピー
                        if(j>=2 && j<=6){
                            //股の角度は反転
                            if(j==5) genes[k, i, j] = -1*genes[k, i-WALKING_SYCLE/2, j+5];
                            else genes[k, i, j] = genes[k, i-WALKING_SYCLE/2, j+5];
                        }
                        //前半の左の動作を右の動作にコピー
                        else if(j>=7){
                            //股の角度は反転
                            if(j==10) genes[k, i, j] = -1*genes[k, i-WALKING_SYCLE/2, j-5];
                            else genes[k, i, j] = genes[k, i-WALKING_SYCLE/2, j-5];
                        } else {
                            genes[k, i, j] = r.Next(0, 2) == 0 ? genes[parent[0], i, j] : genes[parent[1], i, j];
                        }
                    } 
                }
            }
        }
        //上位5体はそのまま次の世代に受け継がれる
        for(int k=SIZE-5; k<SIZE; k++) {
            for(int i=0; i<WALKING_SYCLE; i++) {
                for(int j=0; j<JOINTS; j++){
                    genes[k, i, j] = genes[index[SIZE - k - 1], i, j];
                }
            }
        }
    }
    //シミュレーション終了
    void Quit() {
        UnityEditor.EditorApplication.isPlaying = false;
        UnityEngine.Application.Quit();
    }
}
