using UnityEngine;
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
		public bool FoldedOut { get { return m_FoldedOut; } set { m_FoldedOut = value; } }
		public bool ForceInvalid { get { return m_ForceInvalid; } set { m_ForceInvalid = value; } }
		public List<EditorInfo> EditorInfos { get { return m_EditorInfos; } }


		public EditorContainer(Object target)
		{
			m_Owner = GetTargetOwner(target);

			m_TitleContent = new GUIContent(m_Owner.name);

			m_EditorInfos.Add(new EditorInfo(Editor.CreateEditor(target)));
		}

		public void AddEditorForTarget(Object target)
		{
			if (!m_EditorInfos.Exists(e => e.Editor.target == target))
			{
				m_EditorInfos.Add(new EditorInfo(Editor.CreateEditor(target)));
			}
		}
		
		public bool IsValid()
		{
			return !m_ForceInvalid && m_Owner != null && m_EditorInfos.Count > 0;
		}

		public void RemoveInvalidEditors()
		{
			m_EditorInfos.RemoveAll(e => !e.IsValid());
		}


		public static Object GetTargetOwner(Object target)
		{
			if (target is Component)
			{
				return ((Component)target).gameObject;
			}
			else if (target is ScriptableObject)
			{
				return target;
			}
			else if (target is Material)
			{
				return target;
			}

			return null;
		}
	}
}