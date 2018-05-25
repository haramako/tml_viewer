using System;
using System.Collections.Generic;
using System.Linq;

namespace Tml
{
    /// <summary>
    /// レイアウトを行う
    /// 
    /// 
    /// ブロック要素の場合、縦にエレメントを配置し、Widthを100%にする。
    /// 
    /// インライン要素の場合、横にエレメントを配置し、右に達したら折り返す。
    /// </summary>
    internal class TableLayouter : ICumtomLayouter
    {
        Element target_;

        public TableLayouter(Element target){
            target_ = target;
		}

		/// <summary>
		/// レイアウト情報を計算し直す
		/// </summary>
		public void Reflow()
		{
            target_.Fragments.Clear ();

            var currentY = target_.Style.PaddingTop;
            var currentX = target_.Style.PaddingLeft;
            var idx = 0;
            var maxHeight = 0;

            foreach ( var e in target_.Children)
			{
                if (e.LayoutType != LayoutType.Block)
                {
                    throw new Exception("Table children must be block element");
                }
                var col = idx % target_.Cols.Length;
                var width = target_.Cols[col];
                if( width < 0)
                {
                    width = target_.LayoutedWidth - target_.Style.PaddingRight - currentX;
                }

                // ブロックレイアウトの場合
                // まず、幅を決定してから、高さを計算する
                e.LayoutedWidth = width;

                new Layouter(e).Reflow();

                e.CalculateBlockHeight();
                maxHeight = Math.Max(e.LayoutedHeight, maxHeight);

                e.LayoutedY = currentY;
                e.LayoutedX = currentX;
                currentX += e.LayoutedWidth;

                target_.Fragments.Add(e);

                idx++;

                // 次の行に行く場合
                if( idx % target_.Cols.Length == 0)
                {
                    currentY += maxHeight;
                    currentX = target_.Style.PaddingLeft;
                    maxHeight = 0;
                }
            }

            target_.LayoutedHeight = currentY + maxHeight;
			if (target_.LayoutedHeight < target_.Style.Height) {
                target_.LayoutedHeight = target_.Style.Height;
			}
		}

	}
}

