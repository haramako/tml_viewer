using NUnit.Framework;
using System;
#if USE_SYSTEM_XML
using System.Xml;
#else
using Tml.XmlPolyfill;
#endif

namespace Tml
{
	[TestFixture]
	public class TmlTest
	{
		StyleSheet styleSheet_;

		[SetUp]
		public void SetUp(){
			Tml.Logger.Enabled = true;
			styleSheet_ = new StyleSheet ();
		}

		[Test]
		public void ParseEmptyTagTest()
		{
			string src = @"a<div><img id='a'/></div>b";
			var root = Parser.Default.Parse(src);
			Assert.AreEqual(0, root.FindById ("a").Children.Count);
		}

		[Test]
		public void ParseErrorTest()
		{
			Assert.Throws<XmlException> (() => {
				Parser.Default.Parse("<>");
			});
		}

		[Test]
		public void ParseErrorTest3()
		{
			Assert.Throws<XmlException> (() => {
				Parser.Default.Parse("<div>");
			});
		}

		[Test]
		public void ParseErrorTest2()
		{
			Assert.Throws<Tml.ParserException> (() => {
				Parser.Default.Parse("fuga<hoge></hoge>");
			});
		}

		[Test]
		public void LoaderTest1()
		{
			string src = @"<h1 x=""100"" hoge=""fuga""><p>hoge</p><p>fuga</p></h1>";
			var root = Tml.Parser.Default.Parse(src);
			root.ApplyStyle (styleSheet_);
			Assert.AreNotEqual(root, null);
		}

		[Test]
		public void LoaderTest2()
		{
			string src = @"<div id='div' width='100'><p id='p1'>hoge</p><p id='p2'>fuga</p></div>";
			string css = "p { margin-left: 1; margin-right: 2; margin-top: 3; margin-bottom: 4; }";
			var root = Tml.Parser.Default.Parse(src);
			var sheet = new StyleSheet ();
			new StyleParser ().ParseStyleSheet (sheet, css);
			root.ApplyStyle (sheet);
			root.LayoutedWidth = 100;
			var layouter = new Layouter (root);
			layouter.Reflow();
			var p1 = root.FindById ("p1");
			var p2 = root.FindById ("p2");
			Logger.Log (p1.Id);
			Assert.AreEqual(95, p1.LayoutedWidth);
			Assert.AreEqual(15, p1.LayoutedHeight);
			Assert.AreEqual(15, p2.LayoutedHeight);
			Assert.AreEqual (44, root.LayoutedHeight);
			//Assert.AreEqual(element, null);
		}

		[Test]
		public void ParseAllowMultipleRootTest()
		{
			string src = @"<p>fuga</p><p>hoge</p>";
			var root = Parser.Default.Parse (src);
			Assert.AreEqual (2, root.Children.Count);
		}

		[Test]
		public void ParseAllowRootTextElementTest()
		{
			string src = @"hoge<p>fuga</p>piyo";
			var root = Parser.Default.Parse (src);
			Assert.AreEqual (3, root.Children.Count);
		}

		[Test]
		public void ParseWithStyleTag()
		{
			string src = @"<div id='div'><style>div { margin-top: 10; }</style>hoge</div>";
			var root = Parser.Default.Parse (src);
			var styleDiv = root.StyleSheet.GetStyle ("div");
			var div = root.FindById ("div");
			Assert.AreEqual (10, styleDiv.MarginTop);
			Assert.AreEqual (10, div.Style.MarginTop);
		}

		[Test]
		public void ParseWithStyleTag2()
		{
			string src = @"<style>div { margin-top: 10; }</style><div>hoge</div>";
			Parser.Default.Parse (src);
		}

		[Test]
		public void ParseWithStyleTag3()
		{
			string src = @"<style>div { margin-top: 10; }</style><h1><div></div></h1>";
			Parser.Default.Parse (src);
		}
			}

	[TestFixture]
	public class LayouterTest {
		[SetUp]
		public void SetUp(){
			Tml.Logger.Enabled = true;
		}

		public Document parseAndLayout(string str, int width = 40){
			var root = Parser.Default.Parse(str);
			root.LayoutedWidth = root.Width = width;
			var layouter = new Layouter (root);
			layouter.Reflow();
			return root;
		}

		[Test]
		public void ReflowInlineTest1()
		{
			var root = parseAndLayout ("hogefugapiyo");
			Assert.AreEqual (3, root.Fragments.Count); // hoge / fuga / p iyo
		}

		[Test]
		public void ReflowInlineTest2()
		{
			var root = parseAndLayout("hoge<span>fugap</span>iyo");
			Assert.AreEqual (4, root.Fragments.Count); // hoge / fuga / p / iyo
		}

		[Test]
		public void ReflowInlineBlockTest1()
		{
			var root = parseAndLayout("h<img id='a' width='20' height='20'/>oge");
			var img = root.FindById ("a");
			Assert.AreEqual (10, img.LayoutedX);
			Assert.AreEqual (0, img.LayoutedY);
		}

		[Test]
		public void ReflowTextAlignCenterTest()
		{
			var root = parseAndLayout("<style>p { text-align: center; }</style><p id='a'>a</p>");
			var e = root.FindById ("a").Fragments[0];
			Assert.AreEqual (15, e.LayoutedX);
		}

		[Test]
		public void ReflowTextAlignRightTest()
		{
			var root = parseAndLayout("<style>p { text-align: right; }</style><p id='a'>a</p>");
			var e = root.FindById ("a").Fragments[0];
			Assert.AreEqual (30, e.LayoutedX);
		}

		[Test]
		public void LineHeightTest()
		{
			var root = parseAndLayout("<style>p { line-scale: 3.0; } c { font-size: 20; }</style><p id='a'>a<span>b</span><span class='c'>c</span></p>");
			var e = root.FindById ("a");
			Assert.AreEqual (30, e.Fragments [0].LayoutedHeight);
			Assert.AreEqual (30, e.Fragments [1].LayoutedHeight);
			//Assert.AreEqual (60, e.Fragments [2].LayoutedHeight); // TODO: 文字サイズの子孫影響を整理する
		}

	}

	[TestFixture]
	public class StyleParserTest {

		StyleParser parser;

		[SetUp]
		public void SetUp(){
			Tml.Logger.Enabled = true;
			parser = new StyleParser ();
		}

		public StyleSheet parseSheet(string src){
			var sheet = new StyleSheet ();
			parser.ParseStyleSheet (sheet, src);
			return sheet;
		}

		[Test]
		public void ParseStyleTest(){
			var style = parser.ParseStyle ("margin-left: 10; margin-right: 20;");
			Assert.AreEqual (10, style.MarginLeft);
			Assert.AreEqual (20, style.MarginRight);
		}

		[Test]
		public void ParseMargin1Test(){
			var style = parser.ParseStyle ("margin: 10;");
			Assert.AreEqual (10, style.MarginLeft);
			Assert.AreEqual (10, style.MarginRight);
			Assert.AreEqual (10, style.MarginTop);
			Assert.AreEqual (10, style.MarginBottom);
		}

		[Test]
		public void ParseMargin2Test(){
			var style = parser.ParseStyle ("margin: 10 20;");
			Assert.AreEqual (10, style.MarginTop);
			Assert.AreEqual (10, style.MarginBottom);
			Assert.AreEqual (20, style.MarginLeft);
			Assert.AreEqual (20, style.MarginRight);
		}

		[Test]
		public void ParseStringPropertyTest(){
			var style = parser.ParseStyle ("background-image: 'bg';");
			Assert.AreEqual ("bg", style.BackgroundImage);
		}

		[Test]
		public void ParseColorPropertyTest(){
			var style = parser.ParseStyle ("background-color: #09afAF;");
			Assert.AreEqual ("#09afAF", style.BackgroundColor);
		}

		[Test]
		public void ParseIdentifierPropertyTest(){
			var style = parser.ParseStyle ("background-color: white;");
			Assert.AreEqual ("white", style.BackgroundColor);
		}

		[Test]
		public void BlockCommentTestTest(){
			var sheet = parseSheet ("/* */ p { margin-left: /** * /*/10; }/* */");
			var style = sheet.GetStyle ("p");
			Assert.AreEqual (10, style.MarginLeft);
		}

		[Test]
		public void LineCommentTestTest(){
			var sheet = parseSheet ("//hogehoge \n p { margin-left: // //\n10; }//\n");
			var style = sheet.GetStyle ("p");
			Assert.AreEqual (10, style.MarginLeft);
		}

		[Test]
		public void ParseStyleSheetTest(){
			var sheet = parseSheet("p { margin-left: 10; margin-right: 20; }");
			var style = sheet.GetStyle ("p");
			Assert.AreEqual (10, style.MarginLeft);
			Assert.AreEqual (20, style.MarginRight);
			Assert.AreEqual (Style.Inherit, style.FontSize);
		}


	}

}

