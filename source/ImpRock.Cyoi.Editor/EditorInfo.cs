using UnityEditor;
using UnityEngine;
using System.Reflection;


namespace ImpRock.Cyoi.Editor
{
	using Editor = UnityEditor.Editor;


	[System.Serializable]
	public class EditorInfo
	{	
		[SerializeField] private UnityEditor.Editor m_Editor = null;
		[SerializeField] private bool m_FoldedOut = true;
		[SerializeField] private bool m_ForceInvalid = false;
		
		
		public Editor Editor { get { return m_Editor; } }
		public bool FoldedOut { get { return m_FoldedOut; } set { m_FoldedOut = value; } }
		public bool ForceInvalid { get { return m_ForceInvalid; } set { m_ForceInvalid = value; } }


		public EditorInfo(Editor editor)
		{
			m_Editor = editor;

			//TODO: set MaterialEditor.forceVisible = true
			if (editor.target is Material)
				Editor.GetType()
					.GetProperty("forceVisible", BindingFlags.Instance | BindingFlags.NonPublic)
					.SetValue(editor, true, null);
		}

		public bool IsValid()
		{
			return !m_ForceInvalid && m_Editor != null && m_Editor.target != null;
		}
	}
}
