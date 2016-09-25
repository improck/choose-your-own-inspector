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
		[SerializeField] private bool m_FoldedOut = true;
		[SerializeField] private bool m_ForceInvalid = false;
		[SerializeField] private bool m_IsMaterial = false;
		
		
		public Editor Editor { get { return m_Editor; } }
		public bool FoldedOut { get { return m_FoldedOut; } set { m_FoldedOut = value; } }
		public bool ForceInvalid { get { return m_ForceInvalid; } set { m_ForceInvalid = value; } }
		public bool IsMaterial { get { return m_IsMaterial; } set { m_IsMaterial = value; } }
		

		public EditorInfo(Editor editor)
		{
			m_Editor = editor;

			if (editor.target is Material)
			{
				Editor.GetType()
					.GetProperty("forceVisible", BindingFlags.Instance | BindingFlags.NonPublic)
					.SetValue(editor, true, null);

				IsMaterial = true;
			}
		}

		public bool IsValid()
		{
			return !m_ForceInvalid && m_Editor != null && m_Editor.target != null;
		}
	}
}
