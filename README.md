# TML

Toydea Markup Language


## TODO

* hover対応
* パッド対応
  - MEMO:通常のUIのnavigationは使い物にならない
* Tool Tip
  * 画面の位置に応じて、TIPSの表示位置を変える
* マウスオーバーでカーソル
* 使い回しのリストビュー

* ルビ

* include

* コンポーネント
  * 複雑なものはC#コードに任せる

* float


## コンポーネント

<component id="btn" params="label cls">
  <div class="{{cls}}">
    <div class="btn">
      {{label}}
	  {{content}}
	</div>
  </div>
</component>

<btn label="hoge"/>

<btn>hoge</btn>

{{@btn}}
  <div class="btn" theme="{{theme}}">
    {{image}}
    {{label}}
  </div>
{{/btn}}

<btn>
  <content id="image"></content>
  <content id="label">ほげ</content>
</btn>

{{#buttons}}
{{/buttons}}


## list

<listview>         
<div>hoge</div>       // ブロックエレメントしか置けない
<div>hoge</div>
<div>hoge</div>
<div>hoge</div>
<div>hoge</div>
...
</listview>
