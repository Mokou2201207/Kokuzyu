// ファイル名: RemoveMissingScriptsInSubtree.cs  // クラス名とファイル名を一致させるための注記
// 本スクリプトは Unity エディタ拡張であり、選択したゲームオブジェクト配下の Missing スクリプトのみを削除します

#if UNITY_EDITOR // エディタ専用コンパイルガード
using System.Collections.Generic; // 一時リストを扱うための using
using UnityEditor; // UnityEditor の API を使うための using
using UnityEngine; // GameObject や Transform を扱うための using
using UnityEditor.SceneManagement;
public static class RemoveMissingScriptsInSubtree // エディタメニュー用の静的クラス定義
{
    // ---------------------------------------------
    // メニュー: GameObject 右クリックから実行（単一オブジェクト起点）
    // ---------------------------------------------
    [MenuItem("GameObject/Script無き者をリムーブ", false, 49)] // ヒエラルキー右クリックメニューに項目を追加
    private static void RemoveMissingScripts_Context() // コンテキストメニューの実行本体
    {
        // 現在選択中のオブジェクトを取得（単一選択を想定）
        GameObject selected = Selection.activeGameObject; // アクティブな選択オブジェクトを取得
        if (selected == null) // 選択が無い場合のガード
        {
            EditorUtility.DisplayDialog("Remove Missing Scripts", "GameObject が選択されていません。", "OK"); // ダイアログ表示
            return; // 何もせず終了
        }

        // 実処理を呼び出し（サブツリー全体を対象）
        int removed = RemoveMissingScriptsUnderRoot(selected); // 選択オブジェクト配下で削除したコンポーネント数を受け取る

        // 結果をダイアログ表示
        EditorUtility.DisplayDialog("Remove Missing Scripts", $"対象: {selected.name}\n削除数: {removed}", "OK"); // 処理結果を通知
    }

    // ---------------------------------------------
    // メニュー: Tools から実行（複数選択にも対応）
    // ---------------------------------------------
    [MenuItem("Tools/Cleanup/Remove Missing Scripts In Selection/Subtrees")] // ツールバーの Tools メニューに項目を追加
    private static void RemoveMissingScripts_Selection() // 複数選択に対応した実行本体
    {
        // 選択中の全オブジェクトを取得
        GameObject[] selection = Selection.gameObjects; // 複数選択されたオブジェクト配列を取得
        if (selection == null || selection.Length == 0) // 選択が空のときのガード
        {
            EditorUtility.DisplayDialog("Remove Missing Scripts", "GameObject が選択されていません。", "OK"); // ダイアログ表示
            return; // 終了
        }

        // アンドゥ操作に備えて記録開始
        Undo.IncrementCurrentGroup(); // アンドゥグループを進める
        int totalRemoved = 0; // 総削除数をカウントする変数

        // 各選択オブジェクトごとに処理
        foreach (var go in selection) // 選択配列をループ
        {
            if (go == null) continue; // 念のためヌルチェック
            totalRemoved += RemoveMissingScriptsUnderRoot(go); // 各ルート配下で削除処理を実行し合計へ加算
        }

        // アセットを保存（シーン変更を反映させる）
        AssetDatabase.SaveAssets(); // アセットの保存を促す
        EditorSceneManager.MarkAllScenesDirty(); // シーンを変更あり状態にする（using UnityEditor.SceneManagement が必要）

        // 結果表示
        EditorUtility.DisplayDialog("Remove Missing Scripts", $"選択数: {selection.Length}\n総削除数: {totalRemoved}", "OK"); // 結果ダイアログ
    }

    // ---------------------------------------------
    // 実処理: 指定ルート配下で Missing スクリプトのみ削除
    // ---------------------------------------------
    private static int RemoveMissingScriptsUnderRoot(GameObject root) // ルート GameObject を受け取り削除数を返す関数
    {
        int removedCount = 0; // 削除数カウンタを初期化

        // ルートを含む全子孫 Transform を収集（非アクティブも含む）
        List<Transform> all = new List<Transform>(); // 一時的な Transform 保持リスト
        root.GetComponentsInChildren(true, all); // ルートを含む全ての子孫 Transform を取得

        // アンドゥのためのグループ開始
        int undoGroup = Undo.GetCurrentGroup(); // 現在のアンドゥグループIDを取得

        // 各 GameObject に対して Missing を削除
        foreach (var t in all) // すべての Transform をループ
        {
            if (t == null) continue; // 念のためヌルスキップ
            GameObject go = t.gameObject; // 対象の GameObject を取得

            // 削除前の Missing 数を取得（Editor専用API）
            int before = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go); // Missing スクリプトの数を取得

            if (before > 0) // Missing がある場合のみ処理
            {
                Undo.RegisterCompleteObjectUndo(go, "Remove Missing Scripts"); // アンドゥ登録（ゲームオブジェクトの変更を記録）

                // Missing スクリプトを削除（Editor専用API）
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go); // この一行で Missing の MonoBehaviour を一括削除

                // 削除後の Missing 数を再取得
                int after = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go); // 再度 Missing 数を確認
                removedCount += Mathf.Max(0, before - after); // 実際に削除できた数を加算

                EditorUtility.SetDirty(go); // オブジェクトをダーティにして変更をマーク
            }
        }

        // アンドゥグループを結合して一括操作にまとめる
        Undo.CollapseUndoOperations(undoGroup); // アンドゥ履歴を見やすくまとめる

        return removedCount; // ルート配下で削除した合計数を返す
    }
}

#endif // UNITY_EDITOR の終わり
