using UnityEditor;
using UnityEngine;
using System.Collections.Generic;


namespace ImpRock.Cyoi.Editor
{
	internal sealed class CyoiWindow : EditorWindow
	{
		[SerializeField] private List<EditorContainer> m_EditorContainers = new List<EditorContainer>();
		[SerializeField] private Vector2 m_ScrollPosition = Vector2.zero;

		private bool m_Initialized = false;

		private GUIStyle m_MainBorderStyle = null;
		private GUIStyle m_MainBorderCollapsedStyle = null;
		private GUIStyle m_HeaderBorderStyle = null;
		private GUIStyle m_HeaderFoldoutStyle = null;
		private GUIStyle m_ButtonCloseStyle = null;
		private GUIStyle m_EditorSpacingStyle = null;

		
		public void AddEditorForTarget(Object target)
		{
			Debug.Log(target.GetType());

			Object owner = EditorContainer.GetTargetOwner(target);

			EditorContainer container = m_EditorContainers.Find(c => c.Owner == owner);
			if (container != null)
			{
				container.AddEditorForTarget(target);
			}
			else
			{
				container = new EditorContainer(target);
				m_EditorContainers.Add(container);
			}
		}
		
		private void Initialize()
		{
			if (m_Initialized)
				return;

			m_Initialized = true;
			
			//TODO: distinguish between pro and "personal"
			GUISkin editorSkin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);

			m_MainBorderStyle = new GUIStyle(editorSkin.GetStyle("grey_border"));
			m_MainBorderStyle.name = "MainBorder";
			m_MainBorderStyle.margin = new RectOffset(4, 4, 4, 4);
			m_MainBorderStyle.padding.bottom = 4;

			m_MainBorderCollapsedStyle = new GUIStyle(m_MainBorderStyle);
			m_MainBorderCollapsedStyle.name = "MainBorderCollapsed";
			m_MainBorderCollapsedStyle.padding.bottom = 0;

			m_HeaderBorderStyle = new GUIStyle(editorSkin.box);
			m_HeaderBorderStyle.name = "HeaderBorder";
			m_HeaderBorderStyle.margin = new RectOffset();

			m_HeaderFoldoutStyle = new GUIStyle(editorSkin.GetStyle("IN Foldout"));
			m_HeaderFoldoutStyle.name = "HeaderFoldout";
			m_HeaderFoldoutStyle.fontSize = 12;
			m_HeaderFoldoutStyle.fontStyle = FontStyle.Bold;

			m_ButtonCloseStyle = new GUIStyle(editorSkin.GetStyle("WinBtnClose"));
			m_ButtonCloseStyle.name = "ButtonClose";
			m_ButtonCloseStyle.margin.top = 4;

			m_EditorSpacingStyle = new GUIStyle();
			m_EditorSpacingStyle.name = "EditorSpacing";
			m_EditorSpacingStyle.padding = new RectOffset(12, 4, 0, 0);
		}

		private void OnEnable()
		{
			titleContent.text = "CYOI";
			Vector2 min = minSize;
			min.x = 280.0f;
			minSize = min;
		}

		private void OnGUI()
		{
			Initialize();

			bool hasInvalid = false;

			if (m_EditorContainers.Count == 0)
				return;

			m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition);
			{
				for (int i = 0; i < m_EditorContainers.Count; i++)
				{
					if (m_EditorContainers[i].IsValid())
					{
						GUILayout.BeginVertical(m_EditorContainers[i].FoldedOut ? m_MainBorderStyle : m_MainBorderCollapsedStyle, GUILayout.MinHeight(22.0f));
						{
							if (!m_EditorContainers[i].OwnsSelf || !m_EditorContainers[i].FoldedOut)
							{
								GUILayout.BeginHorizontal(m_HeaderBorderStyle);
								{
									m_EditorContainers[i].FoldedOut = EditorGUILayout.Foldout(m_EditorContainers[i].FoldedOut, m_EditorContainers[i].TitleContent, m_HeaderFoldoutStyle);

									GUILayout.FlexibleSpace();
									if (GUILayout.Button(GUIContent.none, m_ButtonCloseStyle, GUILayout.Width(16.0f), GUILayout.Height(16.0f)))
									{
										m_EditorContainers[i].ForceInvalid = true;
										hasInvalid = true;
									}
								}
								GUILayout.EndHorizontal();
							}

							if (m_EditorContainers[i].FoldedOut)
							{
								if (!m_EditorContainers[i].OwnsSelf)
								{
									List<EditorInfo> infos = m_EditorContainers[i].EditorInfos;
									for (int j = 0; j < infos.Count; j++)
									{
										GUILayout.BeginHorizontal();
										{
											infos[j].FoldedOut = EditorGUILayout.InspectorTitlebar(infos[j].FoldedOut, infos[j].Editor.target);
											if (GUILayout.Button(GUIContent.none, m_ButtonCloseStyle, GUILayout.Width(14.0f), GUILayout.Height(14.0f)))
											{
												infos[j].ForceInvalid = true;
												hasInvalid = true;
											}
										}
										GUILayout.EndHorizontal();

										if (infos[j].FoldedOut)
										{

											infos[j].Editor.OnInspectorGUI();
										}
									}
								}
								else
								{

									m_EditorContainers[i].EditorInfos[0].Editor.DrawHeader();
									m_EditorContainers[i].EditorInfos[0].Editor.OnInspectorGUI();
								}

								//TODO: draw optional preview?
							}
						}
						GUILayout.EndVertical();
					}
					else
					{
						hasInvalid = true;
					}
				}
			}
			GUILayout.EndScrollView();

			if (hasInvalid)
			{
				foreach (EditorContainer container in m_EditorContainers)
				{
					container.RemoveInvalidEditors();
				}

				m_EditorContainers.RemoveAll(c => !c.IsValid());
			}
		}


		[MenuItem("CONTEXT/Component/Add to CYOI", priority = 29, validate = false)]
		[MenuItem("CONTEXT/ScriptableObject/Add to CYOI", priority = 29, validate = false)]
		[MenuItem("CONTEXT/Material/Add to CYOI", priority = 29, validate = false)]
		[MenuItem("CONTEXT/Object/Add to CYOI", priority = 29, validate = false)]
		public static void Context_AddToCyoi(MenuCommand command)
		{
			CyoiWindow window = GetWindow<CyoiWindow>();
			window.AddEditorForTarget(command.context);

			window.Show();
		}
	}
}
