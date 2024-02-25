3DStrategy ゲームデモ
===================================

# ゲームデモ概要

- このゲームデモは、味方ユニットへ戦略を指示することで、敵ユニットを倒していくゲームです。
  - AI制御開発用のデモのため、プレイヤーはユニットへ戦略のみを通知することができ、詳細な動きができるようなUIは作成しておりません

![ゲーム画面](/doc/img/gameImage.png)

- ゲーム内のフィールドは碁盤のように格子上になっています。
  - 将棋やチェスのように、ユニットはマス単位で移動することが可能です。(マスとマスの間にはユニットは存在できません)
  
- 各ユニットは隣接した敵に対して攻撃を仕掛けることができます。
  - 攻撃を繰り返し、敵ユニットが無くなればプレイヤーの勝利。味方ユニットが無くなればプレイヤーの敗北となります。

![ゲーム終了画面](/doc/img/gameFinish.png)

# ゲームデモの流れ

- ゲームは味方ターンと敵ターンを交互に行うことで進行します。

## 味方ターン

- 味方ターンではオレンジ色の髪の味方ユニットにそれぞれ戦略を指示します。
  - 各味方ユニットは指示された戦略に沿って行動を行います。
  - 全ての味方ユニットに戦略を指示したらターン終了です。
  
- 戦略指示はプレイヤーが行います。戦略の指示の仕方は後述する[ゲームデモ操作方法](#ゲームデモ操作方法)にて説明します。

## 敵ターン

- 敵ターンは戦略AIが各敵ユニットに指示を行います。
  - 戦略AIは各敵ユニットに戦略を指示したらターン終了です。

# ユニット

- ユニットは下記のパラメータを持っています。このパラメータを基にユニットは行動します。

| パラメータ | 説明 |
| --- | --- |
| HP | 初期値が100で、この値が0になるとユニットは消滅します |
| 攻撃力 | 他のユニットに攻撃する際に参照されるパラメータ |
| 防御力 | 他のユニットから攻撃を受けるときに参照されるパラメータ |
| 移動力 | 一度の行動でユニットが移動できるマスの数 |

- 味方ユニットはプレイヤーの入力からの戦略指示、敵ユニットは戦略AIからの Attack戦略, Defense 戦略, Balance 戦略, Recovery戦略のどれか一つの戦略で行動します。
  - 指定した戦略指示によるユニットの詳細の動きは [ユニットAI設計](/doc/CharacterAI.md) をご参照ください。

# 攻撃

- 全てのユニットは隣接したユニットに攻撃してHPを減らすことができます。
  - 味方ユニットは敵ユニットへ、敵ユニットは味方ユニットへのみ攻撃します

![攻撃画面](/doc/img/attack.png)

- 攻撃を行う相手のユニットが防御戦略を取っていない場合、攻撃されたユニットは反撃を行います。

# ゲームデモ操作方法

- プレイヤーは戦略選択用のUIから、各味方ユニットに戦略指示を行います。
  - この戦略選択を行うことで敵ユニットを全て消滅させることを目指します。

![戦略指示画面](/doc/img/input.png)

- 上記戦略選択用のUIに表示されているボタンは、先述した[ユニット](#ユニット)の項目で説明した各戦略と下記のように対応しています。
  - Attack → Attack 戦略
  - Defense → Defense 戦略
  - Balance → Balance 戦略
  - Recovery → Recovery 戦略

- また、Playerターンの間は、キーボードの「V」を押すことでViewモードになります。
  - Viewモード中はキーボードの十字キーを押すことで、視点移動とフォーカスしているユニットのパラメータの参照が可能です。

![Viewモード画面](/doc/img/viewMode.png)

- Viewモード中にもう一度キーボードの「V」を押すことによって、Viewモードを終了し、戦略指示の画面に戻ります。

# ゲームデモ設計・開発環境

## 開発環境

| 項目 | 仕様環境 | バージョン |
| --- | --- | --- |
| OS | windows | Windows10 | 
| ゲームエンジン | Unity | 2021.3.27f1 |
| ビルド環境 | Visual Studio | Microsoft Visual Studio Community 2019 |  


## 設計

- このゲームデモに関するシステム設計とスクリプトに関してをここに示します。

- まず、ゲームデモ自体の全体の流れは以下のようになっています。

![ゲームループ](/doc/img/3Dstrategyゲームループ.drawio.png)

具体的なシステム設計に関しては下記のリンク先に示します。

- ゲーム内のオブジェクト図とオブジェクトごとの関連を下記のリンク先にして示します。
  - [オブジェクト設計](/doc/Object.md)

- ゲーム内において、プレイヤーからの入力がユニットへ伝わる流れを示すシーケンスと、戦略家AIの戦略指示がユニットへ伝わる流れを示すシーケンスを下記のリンク先にて示します。
  - [シーケンス設計](/doc/Sequence.md)

- 戦略家AIの設計を下記のリンク先にて示します。
  - [戦略家AI設計](/doc/StrategyAI.md)
  
- ユニットAIの設計を下記のリンク先にて示します。
  - [ユニットAI設計](/doc/CharacterAI.md)

## 使用素材

- ユニット素材
  - https://assetstore.unity.com/packages/3d/characters/humanoids/rpg-tiny-hero-duo-pbr-polyart-225148?locale=ja-JP
  
- UniRx
  - https://github.com/neuecc/UniRx/tree/master
  [こちら](/project/Assets/Plugins/UniRx)に配置しており、ライセンスもこちらを参照のこと
  

