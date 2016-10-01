using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;


namespace ImpRock.Cyoi.Editor
{
	using Editor = UnityEditor.Editor;


	[System.Serializable]
	public class EditorContainer
	{
		[SerializeField] private Object m_Owner = null;
		[SerializeField] private GUIContent m_TitleContent = null;
		[SerializeField] private Editor m_MainEditor = null;
		[SerializeField] private bool m_FoldedOut = true;
		[SerializeField] private bool m_ForceInvalid = false;
		[SerializeField] private InspectorMode m_InspectorMode = InspectorMode.Normal;
		[SerializeField] private List<EditorInfo> m_EditorInfos = new List<EditorInfo>();

		private MethodInfo m_GetIconForObject = null;


		public Object Owner { get { return m_Owner; } }
		public GUIContent TitleContent { get { return m_TitleContent; } }
		public Editor MainEditor { get { return m_MainEditor; } }
		public bool OwnsSelf { get {  return m_Owner != null && m_EditorInfos.Count == 1 && m_Owner == m_EditorInfos[0].Editor.target; } }
		public bool FoldedOut { get { return m_FoldedOut; } set { m_FoldedOut = value; } }
		public bool ForceInvalid { get { return m_ForceInvalid; } set { m_ForceInvalid = value; } }
		public InspectorMode InspectorMode { get { return m_InspectorMode; } set { m_InspectorMode = value; } }
		public List<EditorInfo> EditorInfos { get { return m_EditorInfos; } }


		public EditorContainer(Object owner)
		{
			m_Owner = owner;

			//TODO: set the icon
			m_TitleContent = new GUIContent(m_Owner.name);

			if (m_Owner is GameObject)
			{
				m_MainEditor = Editor.CreateEditor(m_Owner);
			}
		}

		public EditorInfo AddEditorForTarget(Object target)
		{
			EditorInfo editorInfo = m_EditorInfos.Find(e => e.Editor.target == target);
			if (editorInfo == null)
			{
				Editor editor = Editor.CreateEditor(target);
				editorInfo = new EditorInfo(editor);
				m_EditorInfos.Add(editorInfo);

				if (editorInfo.Editor.RequiresConstantRepaint())
					CyoiWindow.RequiresContantUpdateCounter++;

				if (m_Owner == target && editorInfo.Editor.target is AssetImporter)
				{
					m_TitleContent.text = editorInfo.EditorTitle;
				}

				return editorInfo;
			}

			return null;
		}

		public void RefreshTitle()
		{
			if (!OwnsSelf)
			{
				//TODO: set the icon
				m_TitleContent.text = m_Owner.name;
			}
		}
		
		public bool IsValid()
		{
			return !m_ForceInvalid && m_Owner != null && m_EditorInfos.Count > 0 && m_EditorInfos.TrueForAll(e => e.IsValid());
		}

		public void RemoveInvalidEditors()
		{
			for (int i = m_EditorInfos.Count - 1; i >= 0; i--)
			{
				EditorInfo editorInfo = m_EditorInfos[i];
				if (!editorInfo.IsValid())
				{
					m_EditorInfos.RemoveAt(i);
					if (editorInfo.Editor != null)
					{
						if (editorInfo.Editor.RequiresConstantRepaint())
							CyoiWindow.RequiresContantUpdateCounter--;

						Object.DestroyImmediate(editorInfo.Editor);
					}
				}
			}
		}

		private Texture2D GetIconForObject(Object target)
		{
			if (m_GetIconForObject == null)
			{
				m_GetIconForObject = typeof(EditorGUIUtility).GetMethod("GetIconForObject", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
			}

			object icon = m_GetIconForObject.Invoke(null, new object[] { target });

			return icon as Texture2D;
		}
	}
}