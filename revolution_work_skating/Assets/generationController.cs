using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class generationController : MonoBehaviour
{
    public GameObject score_object = null; // Textオブジェクト
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // オブジェクトからTextコンポーネントを取得
        Text score_text = score_object.GetComponent<Text>();
        // テキストの表示を入れ替える
        score_text.text = "GENERATION : " + (RigControl2.GENERATION + 1).ToString() + '\n' +
        "MAX SCORE : " + ((Math.Floor(RigControl2.MAX * 10)) / 10).ToString() + '\n' +
        "AVG SCORE : " + ((Math.Floor(RigControl2.AVG_SCORE * 10)) / 10).ToString();
    }
}
