using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AutoResourceCrawler : EditorWindow
{
    //�E�B���h�E�\���p
    [MenuItem("Window/CrawlerWindow/AutoResourceCrawler")]
    static void OpenWindow() {
        var window = GetWindow<AutoResourceCrawler>();
        window.maxSize = new Vector2(200, 100);
        window.minSize = new Vector2(200, 100);
        window.Show();
    }


    /////////////////////////////////////////////////////////
    //�蓮��������
    bool isAutoCrawl = false;

    //���s�Ԋu
    const float cIntervalSecond = 3;
          float passedTime      = 0;

    //�{��
    Crawler crawler;

    //�Ď���t�H���_��
    const string cFolderPathObserve         = @"�D���ȃt�H���_�����ĂˁI";
    //�Ď�����g���q
    readonly string[] observeExtends        = new string[] { ".jpg", ".png", ".fbx", ".wav" };
    //unity���̃R�s�[�惋�[�g�t�H���_��
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
        //���s���͍X�V���Ȃ�
        if (Application.isPlaying) return;


        //��莞�Ԗ��ɃN���[���[�����s
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

        //�������s���͎蓮�֎~
        EditorGUI.BeginDisabledGroup(isAutoCrawl);

        if(GUILayout.Button("let's crawl once")) {
            if (crawler != null) {
                crawler.run();
            }
        }

        EditorGUI.EndDisabledGroup();
    }

}
