using UnityEditor;
using UnityEngine;
using System.Reflection;


namespace ImpRock.Cyoi.Editor
{
	using Editor = UnityEditor.Editor;


	[System.Serializable]
	public class EditorInfo
	{	
		[SerializeField] private Editor m_Editor = null;
		[SerializeField] private Editor m_SubEditor = null;
		[SerializeField] private bool m_FoldedOut = true;
		[SerializeField] private bool m_ForceInvalid = false;
		[SerializeField] private bool m_DrawSubEditor = false;
		

		public Editor Editor { get { return m_Editor; } }
		public Editor SubEditor { get { return m_SubEditor; } }
		public bool FoldedOut { get { return m_FoldedOut; } set { m_FoldedOut = value; } }
		public bool ForceInvalid { get { return m_ForceInvalid; } set { m_ForceInvalid = value; } }
		public bool DrawSubEditor { get { return m_DrawSubEditor; } }
		
		public string EditorTitle
		{
			get
			{
				return (string)m_Editor.GetType()
					.GetProperty("targetTitle", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
					.GetValue(m_Editor, null);
			}
		}
		

		public EditorInfo(Editor editor)
		{
			m_Editor = editor;

			if (editor.target is Material)
			{
				editor.GetType()
					.GetProperty("forceVisible", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
					.SetValue(editor, true, null);
			}
			else if (editor.target is AssetImporter)
			{
				//TODO: support sub assets
				AssetImporter importer = (AssetImporter)editor.target;
				Object imported = AssetDatabase.LoadAssetAtPath(importer.assetPath, typeof(Object));
				if (imported != null)
				{
					//TODO: assetEditor value does not get serialized by the AssetImporter
					m_SubEditor = Editor.CreateEditor(imported);
					//TODO: remove this
					Debug.Log(imported.GetType() + " -> " + m_SubEditor.GetType());
					editor.GetType()
						.GetProperty("assetEditor", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
						.SetValue(editor, m_SubEditor, null);

					m_DrawSubEditor = (bool)editor.GetType()
						.GetProperty("showImportedObject", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
						.GetValue(editor, null);
				}
			}
		}

		public bool IsValid()
		{
			return !m_ForceInvalid && m_Editor != null && m_Editor.target != null;
		}

		public void SetInspectorMode(InspectorMode inspectorMode)
		{
			m_Editor.GetType()
				.GetField("m_InspectorMode", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
				.SetValue(m_Editor, inspectorMode);

			m_Editor.serializedObject.GetType()
				.GetProperty("inspectorMode", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
				.SetValue(m_Editor.serializedObject, inspectorMode, null);

			if (m_SubEditor != null)
			{
				m_SubEditor.GetType()
					.GetField("m_InspectorMode", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
					.SetValue(m_SubEditor, inspectorMode);

				m_SubEditor.serializedObject.GetType()
					.GetProperty("inspectorMode", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
					.SetValue(m_SubEditor.serializedObject, inspectorMode, null);
			}
		}
	}
}
