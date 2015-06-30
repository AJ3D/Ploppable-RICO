using ColossalFramework;
using System;
using UnityEngine;
using ICities;

namespace PloppableAI
{

	public class LargeMonument : MonumentAI

	{
		public override void GetWidthRange (out int minWidth, out int maxWidth)
		{
			base.GetWidthRange (out minWidth, out maxWidth);
			minWidth = 1;
			maxWidth = 24;
		}
		public override void GetLengthRange (out int minLength, out int maxLength)
		{
			base.GetLengthRange (out minLength, out maxLength);
			minLength = 1;
			maxLength = 24;
		}
		public override void GetDecorationArea (out int width, out int length, out float offset)
		{
			width = 24;
			length = 24;
			offset = 0f;
		}
	}

}