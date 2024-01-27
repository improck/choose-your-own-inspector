using UnityEditor;
using UnityEngine;


namespace ImpRock.Cyoi.Editor
{
	internal sealed class GraphicAssets
	{
		#region Singleton
		private static GraphicAssets s_Instance = null;

		public static GraphicAssets Instance { get { if (s_Instance == null) { s_Instance = new GraphicAssets(); } return s_Instance; } }

		
		private GraphicAssets()
		{
			InitAssets();
		}

		public static void DestroyInstance()
		{
			if (s_Instance != null)
			{
				s_Instance = null;
			}
		}
		#endregion

		//***** IMAGES *****

		//none

		//***** GUI STYLES *****

		public GUIStyle MainBorderStyle { get; private set; }
		public GUIStyle MainBorderCollapsedStyle { get; private set; }
		public GUIStyle HeaderBorderStyle { get; private set; }
		public GUIStyle HeaderFoldoutStyle { get; private set; }
		public GUIStyle HeaderBackgroundStyle { get; private set; }
		public GUIStyle ButtonCloseStyle { get; private set; }
		public GUIStyle SubEditorHeaderStyle { get; private set; }
		public GUIStyle EditorSpacingStyle { get; private set; }

		//***** COLORS *****

		//none

		//***** CONSTANTS *****

		public const bool ForceProSkin = false;


		public void InitGuiStyle()
		{
			GUISkin editorSkin = null;
			if (EditorGUIUtility.isProSkin || ForceProSkin)
				editorSkin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene);
			else
				editorSkin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);

			MainBorderStyle = new GUIStyle(editorSkin.GetStyle("grey_border"));
			MainBorderStyle.name = "MainBorder";
			MainBorderStyle.margin = new RectOffset(4, 4, 4, 4);
			MainBorderStyle.padding.bottom = 4;

			MainBorderCollapsedStyle = new GUIStyle(MainBorderStyle);
			MainBorderCollapsedStyle.name = "MainBorderCollapsed";
			MainBorderCollapsedStyle.padding.bottom = 0;

			HeaderBorderStyle = new GUIStyle(editorSkin.GetStyle("IN BigTitle"));
			HeaderBorderStyle.name = "HeaderBorder";
			HeaderBorderStyle.margin = new RectOffset(1, 1, 0, 0);
			HeaderBorderStyle.padding = new RectOffset(3, 3, 3, 3);

			HeaderFoldoutStyle = new GUIStyle(editorSkin.GetStyle("IN Foldout"));
			HeaderFoldoutStyle.name = "HeaderFoldout";
			HeaderFoldoutStyle.fontSize = 12;
			HeaderFoldoutStyle.fontStyle = FontStyle.Bold;
			HeaderFoldoutStyle.focused.textColor = HeaderFoldoutStyle.normal.textColor;
			HeaderFoldoutStyle.onFocused.textColor = HeaderFoldoutStyle.normal.textColor;
			HeaderFoldoutStyle.active.textColor = HeaderFoldoutStyle.normal.textColor;
			HeaderFoldoutStyle.onActive.textColor = HeaderFoldoutStyle.normal.textColor;

			HeaderBackgroundStyle = new GUIStyle(editorSkin.GetStyle("IN BigTitle"));
			HeaderBackgroundStyle.name = "HeaderBackground";
			HeaderBackgroundStyle.padding.left = 1;
			HeaderBackgroundStyle.padding.right = 1;
			
			ButtonCloseStyle = new GUIStyle(editorSkin.GetStyle("WinBtnClose"));
			ButtonCloseStyle.name = "ButtonClose";
			ButtonCloseStyle.margin.top = 4;

			SubEditorHeaderStyle = new GUIStyle(editorSkin.GetStyle("OL Title"));

			EditorSpacingStyle = new GUIStyle();
			EditorSpacingStyle.name = "EditorSpacing";
			EditorSpacingStyle.padding = new RectOffset(1, 1, 0, 0);
		}

		public void InitAssets()
		{
			//n/a											
		}
	}
}
