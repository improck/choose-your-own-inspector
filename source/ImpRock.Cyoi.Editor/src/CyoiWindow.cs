using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;


namespace ImpRock.Cyoi.Editor
{
	public sealed class CyoiWindow : EditorWindow
	{
		internal static int RequiresContantUpdateCounter = 0;


		[SerializeField] private List<EditorContainer> m_EditorContainers = new List<EditorContainer>();
		[SerializeField] private Vector2 m_ScrollPosition = Vector2.zero;

		[System.NonSerialized] private bool m_Initialized = false;
		private double m_LastRepaintTime = 0.0;
		
		private const double ConstantRepaintFrameTime = 0.03;

		
		public void AddEditorForTarget(Object target)
		{
			if (target == null)
				return;

			Object owner = CyoiWindow.GetTargetOwner(target);
			EditorContainer container = m_EditorContainers.Find(c => c.Owner == owner);
			if (container == null)
			{
				container = new EditorContainer(owner);
				m_EditorContainers.Add(container);
			}

			EditorInfo editorInfo = container.AddEditorForTarget(target);
			if (editorInfo != null)
			{
				editorInfo.Window = this;

				List<UnityEvent> repaintableEvents = editorInfo.GetRepaintableEvents();
				for (int i = 0; i < repaintableEvents.Count; i++)
				{
					Debug.Log("adding repaint as a listener for " + editorInfo.EditorTitle);
					repaintableEvents[i].AddListener(Repaint);
				}
			}
		}
		
		private void Initialize()
		{
			if (m_Initialized)
				return;

			m_Initialized = true;
			
			GraphicAssets.Instance.InitGuiStyle();
		}

		private void OnEnable()
		{
			CyoiResources.Instance.LoadResources();

			titleContent = new GUIContent("CYOI", CyoiResources.Instance.GetImage(ResId.ImageTabIcon));
			Vector2 min = minSize;
			min.x = 280.0f;
			minSize = min;

			EditorApplication.hierarchyWindowChanged += CleanupEditorContainers;
			EditorApplication.projectWindowChanged += CleanupEditorContainers;

			if (m_EditorContainers.Count > 0)
			{
				foreach (EditorContainer container in m_EditorContainers)
				{
					if (container == null || !container.IsValid())
						continue;

					foreach (EditorInfo info in container.EditorInfos)
					{
						info.Window = this;
					}
				}
			}
		}

		private void OnDisable()
		{
			if (m_EditorContainers.Count > 0)
			{
				foreach (EditorContainer container in m_EditorContainers)
				{
					if (container == null || !container.IsValid())
						continue;

					foreach (EditorInfo info in container.EditorInfos)
					{
						List<UnityEvent> repaintableEvents = info.GetRepaintableEvents();
						for (int i = 0; i < repaintableEvents.Count; i++)
						{
							Debug.Log("removing repaint as a listener for " + info.EditorTitle);
							repaintableEvents[i].RemoveListener(Repaint);
						}
					}
				}
			}

			EditorApplication.hierarchyWindowChanged -= CleanupEditorContainers;
			EditorApplication.projectWindowChanged -= CleanupEditorContainers;
		}
		
		private void OnGUI()
		{
			Initialize();

			bool hasInvalid = false;

			m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition);
			{
				for (int i = 0; i < m_EditorContainers.Count; i++)
				{
					if (m_EditorContainers[i].IsValid())
					{
						GUILayout.BeginVertical(m_EditorContainers[i].FoldedOut ? GraphicAssets.Instance.MainBorderStyle : GraphicAssets.Instance.MainBorderCollapsedStyle, GUILayout.MinHeight(22.0f));
						{
							GUILayout.BeginHorizontal(GraphicAssets.Instance.HeaderBorderStyle);
							{
								m_EditorContainers[i].RefreshTitle();

								Rect foldoutRect = GUILayoutUtility.GetRect(16.0f, 18.0f);
								m_EditorContainers[i].FoldedOut = EditorGUI.Foldout(foldoutRect, m_EditorContainers[i].FoldedOut, m_EditorContainers[i].TitleContent, GraphicAssets.Instance.HeaderFoldoutStyle);
								
								GUILayout.FlexibleSpace();
								if (GUILayout.Button(GUIContent.none, GraphicAssets.Instance.ButtonCloseStyle, GUILayout.Width(16.0f), GUILayout.Height(16.0f)))
								{
									m_EditorContainers[i].ForceInvalid = true;
									hasInvalid = true;
								}
							}
							GUILayout.EndHorizontal();

							if (!m_EditorContainers[i].OwnsSelf)
							{
								if (m_EditorContainers[i].MainEditor != null && m_EditorContainers[i].FoldedOut)
								{
									GUILayout.BeginVertical(GraphicAssets.Instance.EditorSpacingStyle);
									{
										m_EditorContainers[i].MainEditor.DrawHeader();
									}
									GUILayout.EndVertical();

									//have to undo the damage from DrawHeader
									EditorGUIUtility.fieldWidth = 0.0f;
									EditorGUIUtility.labelWidth = 0.0f;
								}
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
											if (GUILayout.Button(GUIContent.none, GraphicAssets.Instance.ButtonCloseStyle, GUILayout.Width(14.0f), GUILayout.Height(14.0f)))
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
									EditorInfo info = m_EditorContainers[i].EditorInfos[0];

									GUILayout.BeginVertical(GraphicAssets.Instance.EditorSpacingStyle);
									{
										info.Editor.DrawHeader();
										
										EditorGUIUtility.fieldWidth = 0.0f;
										EditorGUIUtility.labelWidth = 0.0f;
									}
									GUILayout.EndVertical();

									info.Editor.OnInspectorGUI();
									
									if (info.DrawSubEditor)
									{
										GUILayout.BeginVertical(GraphicAssets.Instance.EditorSpacingStyle);
										{
											//TODO: should there be better text here?
											GUILayout.Label("Imported Object", GraphicAssets.Instance.SubEditorHeaderStyle);
											info.SubEditor.DrawHeader();

											EditorGUIUtility.fieldWidth = 0.0f;
											EditorGUIUtility.labelWidth = 0.0f;
										}
										GUILayout.EndVertical();

										info.SubEditor.OnInspectorGUI();
									}
									
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
				CleanupEditorContainers();
			}
		}

		private void Update()
		{
			if (CyoiWindow.RequiresContantUpdateCounter > 0
				&& m_LastRepaintTime + ConstantRepaintFrameTime < EditorApplication.timeSinceStartup)
			{
				m_LastRepaintTime = EditorApplication.timeSinceStartup;
				Repaint();
			}
		}

		private void CleanupEditorContainers()
		{
			foreach (EditorContainer container in m_EditorContainers)
			{
				container.RemoveInvalidEditors();
			}

			m_EditorContainers.RemoveAll(c => !c.IsValid());

			Repaint();
		}

		private static Object GetTargetOwner(Object target)
		{
			if (target is Component)
			{
				return ((Component)target).gameObject;
			}
			else
			{
				return target;
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
