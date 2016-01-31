using UnityEngine;


namespace ImpRock.MultiEdit.Editor
{
	[System.Serializable]
	public class EditorInfo
	{	
		[SerializeField] private UnityEditor.Editor m_Editor = null;
		[SerializeField] private bool m_FoldedOut = true;
		[SerializeField] private bool m_ForceInvalid = false;
		
		
		public UnityEditor.Editor Editor { get { return m_Editor; } }
		public bool FoldedOut { get { return m_FoldedOut; } set { m_FoldedOut = value; } }
		public bool ForceInvalid { get { return m_ForceInvalid; } set { m_ForceInvalid = value; } }


		public EditorInfo(UnityEditor.Editor editor)
		{
			m_Editor = editor;
		}

		public bool IsValid()
		{
			return !m_ForceInvalid && m_Editor != null && m_Editor.target != null;
		}
	}
}
