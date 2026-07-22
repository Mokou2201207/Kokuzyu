# コードスタイル・コメント共通ルール

## 1. コードコメントの禁止文字
- コード内のコメント（`///` や `//`）には、感嘆符「！」や疑問符「？」を絶対に使用しないでください。

## 2. コメント内のカッコ書き禁止
- コメント内に全角・半角のカッコ `（）` や `()` を入れないでください。

## 3. コメントの位置
- コメントは対象コードの**直前の行**に配置してください。
- コードと同じ行の右側（行末コメント）に配置することは禁止です。

**OK:**
```csharp
// レイヤーを取得
int localPlayerLayer = LayerMask.NameToLayer("LocalPlayer");

// モデルをLocalPlayerに変更
foreach (var model in visualModels)
```

**NG:**
```csharp
int localPlayerLayer = LayerMask.NameToLayer("LocalPlayer"); // レイヤーを取得

foreach (var model in visualModels) // モデルをLocalPlayerに変更
```

## 4. Header属性とSerializeField属性の記述スタイル
- `Header` 属性と `SerializeField` 属性を両方付与する場合、同一のブラケット `[...]` 内にカンマ区切りで記述し、変数の宣言は改行した次の行に記述してください。

**OK:**
```csharp
[Header("見えなくする自分の3Dモデル"), SerializeField]
private GameObject[] visualModels;
```

**NG:**
```csharp
[Header("見えなくする自分の3Dモデル")]
[SerializeField] private GameObject[] visualModels;
```
