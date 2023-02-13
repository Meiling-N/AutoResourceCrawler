using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crawler {
    //監視先
    readonly string observeDirectoryAbsolutePath;
    //監視拡張子
    readonly string[] observeExtends;
    //unity内のコピー先
    readonly string applicationDataPath;
    readonly string unityRootDirectoryName;

    public Crawler(
                string applicationDataPath_,
                string observeDirectoryAbsolutePath_,
                string[] observeExtends_,
                string unityRootFolderName_ = "SakuseiButu\\"
           )
    {

        applicationDataPath          = applicationDataPath_;
        observeDirectoryAbsolutePath = observeDirectoryAbsolutePath_;
        observeExtends               = observeExtends_;
        unityRootDirectoryName       = unityRootFolderName_;


        //unity内のコピー先がないなら作る
        if (!Directory.Exists(getUnityRootPathCopyTo())) {
            Directory.CreateDirectory(getUnityRootPathCopyTo());
        }
    }


    //unityの "Assets/名前" をコピー場所とする
    string getUnityRootPathCopyTo() {
        return Path.Combine(applicationDataPath, unityRootDirectoryName);
    }

    string makeUnityPathFromOriginal(string originalFileFullPath) {
        //元の奴から監視ルートパスを削ると欲しいパスが手に入る
        //あとはunity用のパスにくっつけて終わり
        var relativePath = originalFileFullPath.Substring(observeDirectoryAbsolutePath.Length);
        return Path.Combine(getUnityRootPathCopyTo(), relativePath);
    }

    string makeOriginalPathFromUnity(string unityFileFullPath) {
        var relativePath = unityFileFullPath.Substring(getUnityRootPathCopyTo().Length);
        return Path.Combine(observeDirectoryAbsolutePath, relativePath);
    }

    public void run() {
        //監視先がないなら終了
        if (!Directory.Exists(observeDirectoryAbsolutePath)) {
            Debug.LogWarning("Can't Find Observe Directory... Is this Correct Path? : " + observeDirectoryAbsolutePath);
            return;
        }

        copyProcess();
        deleteProcess();
    }

    /// <summary>
    /// 監視フォルダからunityへコピー
    /// </summary>
    void copyProcess() {
        //全フォルダが揃ってるか確認
        var observeDirectries = Directory.GetDirectories(observeDirectoryAbsolutePath, "*", SearchOption.AllDirectories);
        foreach (var d in observeDirectries) {
            //ないなら作る
            if (!isThereUnityDirectory(d)) {
                var unityDir = makeUnityPathFromOriginal(d);
                Directory.CreateDirectory(unityDir);
                Debug.Log("Create Unity Directory: " + unityDir);
            }
        }

        //ルートフォルダー内にある全ての必要なファイルをとる
        var entries      = Directory.GetFileSystemEntries(observeDirectoryAbsolutePath, "*.*", SearchOption.AllDirectories);
        var observeFiles = entries.Where(name => observeExtends.Contains(Path.GetExtension(name)));

        //(unity側にない || あるが、監視先の方が新しい) 場合はコピー
        foreach (var observePath in observeFiles) {
            var unityPath = makeUnityPathFromOriginal(observePath);
            if (!File.Exists(unityPath) || isObserveFileLatest(observePath, unityPath)) {
                File.Copy(observePath, unityPath, true);
                Debug.Log("Copy File to: " + unityPath);
            }
        }
    }

    /// <summary>
    /// unity側にはあるが監視フォルダにはない奴の消去
    /// </summary>
    void deleteProcess() {
        var unityRootPath = getUnityRootPathCopyTo();

        //全フォルダが存在しているか確認
        var unityDirectries = Directory.GetDirectories(unityRootPath, "*", SearchOption.AllDirectories);
        foreach (var uniD in unityDirectries) {
            //ないならunity側を消す
            if (!isThereOriginalDirectory(uniD) && Directory.Exists(uniD)) {
                Directory.Delete(uniD, true);
                deleteMetaFile(uniD);
                Debug.Log("Delete Unity Directory: " + uniD);
            }
        }

        //全ファイルが存在しているか確認
        var entries    = Directory.GetFileSystemEntries(unityRootPath, "*.*", SearchOption.AllDirectories);
        var unityFiles = entries.Where(name => observeExtends.Contains(Path.GetExtension(name)));
        
        //(監視側にない && unity側にある) 場合はunity側を消去
        foreach (var unityPath in unityFiles) {
            var originalPath = makeOriginalPathFromUnity(unityPath);
            if (!File.Exists(originalPath) && File.Exists(unityPath)) {
                File.Delete(unityPath);
                deleteMetaFile(unityPath);
                Debug.Log("Delete Unity File: " + unityPath);
            }
        }
    }

    bool isThereOriginalDirectory(string unityFolderPath) {
        var orgPath = makeOriginalPathFromUnity(unityFolderPath);
        return Directory.Exists(orgPath);
    }

    bool isThereUnityDirectory(string originalFolderPath) {
        var unityPath = makeUnityPathFromOriginal(originalFolderPath);
        return Directory.Exists(unityPath);
    }

    bool isObserveFileLatest(string observeFilePath,string targetFilePath) {
        var observeDate = File.GetLastWriteTime(observeFilePath);
        var targetDate  = File.GetLastWriteTime(targetFilePath);
        return observeDate > targetDate;
    }
    
    void deleteMetaFile(string targetPath) {
        targetPath += ".meta";
        if (File.Exists(targetPath)) {
            File.Delete(targetPath);
        }
    }
}
