using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AutoResourceCrawler : EditorWindow
{
    //ウィンドウ表示用
    [MenuItem("Window/CrawlerWindow/AutoResourceCrawler")]
    static void OpenWindow() {
        var window = GetWindow<AutoResourceCrawler>();
        window.maxSize = new Vector2(200, 100);
        window.minSize = new Vector2(200, 100);
        window.Show();
    }


    /////////////////////////////////////////////////////////
    //手動か自動か
    bool isAutoCrawl = false;

    //実行間隔
    const float cIntervalSecond = 3;
          float passedTime      = 0;

    //本体
    Crawler crawler;

    //監視先フォルダ名
    const string cFolderPathObserve         = @"好きなフォルダを入れてね！";
    //監視する拡張子
    readonly string[] observeExtends        = new string[] { ".jpg", ".png", ".fbx", ".wav" };
    //unity側のコピー先ルートフォルダ名
    const string cCopytoUnityRootFolderName = "SakuseiButu\\";

    public AutoResourceCrawler() {
    }

    private void OnEnable() {
        crawler = new Crawler(
                    Application.dataPath,
                    cFolderPathObserve,
                    observeExtends,
                    cCopytoUnityRootFolderName
                  );
        passedTime = 0;
    }


    private void Update() {
        if (!isAutoCrawl) return;
        //実行中は更新しない
        if (Application.isPlaying) return;


        //一定時間毎にクローラーを実行
        passedTime += Time.deltaTime;
        if (passedTime < cIntervalSecond) return;
        passedTime -= cIntervalSecond;

        if(crawler != null) {
            crawler.run();
        }
    }


    private void OnGUI() {
        isAutoCrawl = EditorGUILayout.Toggle("isAutoCrawl", isAutoCrawl);
        EditorGUILayout.Space(30);

        //自動実行中は手動禁止
        EditorGUI.BeginDisabledGroup(isAutoCrawl);

        if(GUILayout.Button("let's crawl once")) {
            if (crawler != null) {
                crawler.run();
            }
        }

        EditorGUI.EndDisabledGroup();
    }

}
