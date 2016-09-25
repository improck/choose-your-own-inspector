﻿using UnityEngine;
using System.Collections.Generic;


namespace ImpRock.Cyoi.Editor
{
	using Editor = UnityEditor.Editor;


	[System.Serializable]
	public class EditorContainer
	{
		[SerializeField] private Object m_Owner = null;
		[SerializeField] private GUIContent m_TitleContent = null;
		[SerializeField] private bool m_FoldedOut = true;
		[SerializeField] private bool m_ForceInvalid = false;
		[SerializeField] private List<EditorInfo> m_EditorInfos = new List<EditorInfo>();


		public Object Owner { get { return m_Owner; } }
		public GUIContent TitleContent { get { return m_TitleContent; } }
		public bool OwnsSelf { get {  return m_Owner != null && m_EditorInfos.Count > 0 && m_Owner == m_EditorInfos[0].Editor.target; } }
		public bool FoldedOut { get { return m_FoldedOut; } set { m_FoldedOut = value; } }
		public bool ForceInvalid { get { return m_ForceInvalid; } set { m_ForceInvalid = value; } }
		public List<EditorInfo> EditorInfos { get { return m_EditorInfos; } }


		public EditorContainer(Object owner)
		{
			m_Owner = owner;
			m_TitleContent = new GUIContent(m_Owner.name);
		}

		public void AddEditorForTarget(Object target)
		{
			if (!m_EditorInfos.Exists(e => e.Editor.target == target))
			{
				Editor editor = Editor.CreateEditor(target);
				//TODO: remove this
				Debug.Log(target.GetType() + " -> " + editor.GetType());
				EditorInfo editorInfo = new EditorInfo(editor);
				m_EditorInfos.Add(editorInfo);

				if (editorInfo.Editor.RequiresConstantRepaint())
					CyoiWindow.RequiresContantUpdateCounter++;
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
	}
}