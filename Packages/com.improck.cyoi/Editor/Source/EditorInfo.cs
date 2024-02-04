using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using UnityEngine.Events;


namespace ImpRock.Cyoi.Editor
{
	using Editor = UnityEditor.Editor;


	[System.Serializable]
	public class EditorInfo : ISerializationCallbackReceiver
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
		public CyoiWindow Window { get; set; }
		
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

			if (m_Editor.RequiresConstantRepaint())
			{
				CyoiWindow.RequiresConstantUpdateCounter++;
			}

			System.Type editorType = m_Editor.GetType();

			if (m_Editor.target is Material)
			{
#if UNITY_2019_1_OR_NEWER
				UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(m_Editor.target, true);
#else
				FieldInfo isVisible = editorType.GetField("m_IsVisible", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
				if (isVisible != null)
				{
					isVisible.SetValue(m_Editor, true);
					UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(m_Editor.target, true);
				}
#endif
			}
			else if (m_Editor.target is AssetImporter)
			{
				AssetImporter importer = (AssetImporter)m_Editor.target;
				Object imported = AssetDatabase.LoadAssetAtPath(importer.assetPath, typeof(Object));
				if (imported != null)
				{
					MethodInfo setImportEditor = editorType.GetMethod("InternalSetAssetImporterTargetEditor", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
					if (setImportEditor != null)
					{
						m_SubEditor = Editor.CreateEditor(imported);
						setImportEditor?.Invoke(m_Editor, new object[] { m_SubEditor });
						m_DrawSubEditor = (m_Editor as AssetImporterEditor).showImportedObject;
					}
				}
			}
		}

		public bool IsValid()
		{
			return !m_ForceInvalid && m_Editor != null && m_Editor.target != null;
		}

		public List<UnityEvent> GetRepaintableEvents()
		{
			List<UnityEvent> unityEvents = new List<UnityEvent>();

			if (m_Editor == null)
				return unityEvents;

			//TODO: find fields and properties of type Anim* and subscribe repaints to them
			//NOTE: this assumes that all initialization has happened already, such as OnEnable()
			//TODO: valueChanged is marked NonSerialized!
			FieldInfo[] fieldInfos = m_Editor.GetType()
				.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
			for (int i = 0; i < fieldInfos.Length; i++)
			{
				if (IsBaseAnimValueType(fieldInfos[i].FieldType))
				{
					object fieldInstance = fieldInfos[i].GetValue(m_Editor);
					UnityEvent valueChanged = (UnityEvent)fieldInfos[i].FieldType
						.GetField("valueChanged", BindingFlags.Public | BindingFlags.Instance)
						.GetValue(fieldInstance);

					unityEvents.Add(valueChanged);
				}
				else if (fieldInfos[i].FieldType.IsArray && IsBaseAnimValueType(fieldInfos[i].FieldType.GetElementType()))
				{
					System.Array array = fieldInfos[i].GetValue(m_Editor) as System.Array;
					if (array != null)
					{
						foreach (object element in array)
						{
							if (element == null)
								continue;

							UnityEvent valueChanged = (UnityEvent)element.GetType()
								.GetField("valueChanged", BindingFlags.Public | BindingFlags.Instance)
								.GetValue(element);

							unityEvents.Add(valueChanged);
						}
					}
				}
			}

			if (m_SubEditor != null)
			{
				fieldInfos = m_SubEditor.GetType()
					.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
				for (int i = 0; i < fieldInfos.Length; i++)
				{
					if (IsBaseAnimValueType(fieldInfos[i].FieldType))
					{
						object fieldInstance = fieldInfos[i].GetValue(m_SubEditor);
						UnityEvent valueChanged = (UnityEvent)fieldInfos[i].FieldType
							.GetField("valueChanged", BindingFlags.Public | BindingFlags.Instance)
							.GetValue(fieldInstance);

						unityEvents.Add(valueChanged);
					}
					else if (fieldInfos[i].FieldType.IsArray && IsBaseAnimValueType(fieldInfos[i].FieldType.GetElementType()))
					{
						System.Array array = fieldInfos[i].GetValue(m_SubEditor) as System.Array;
						if (array != null)
						{
							foreach (object element in array)
							{
								if (element == null)
									continue;

								UnityEvent valueChanged = (UnityEvent)element.GetType()
									.GetField("valueChanged", BindingFlags.Public | BindingFlags.Instance)
									.GetValue(element);

								unityEvents.Add(valueChanged);
							}
						}
					}
				}
			}

			return unityEvents;
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

		private bool IsBaseAnimValueType(System.Type subclassType)
		{
			System.Type rawObjectType = typeof(object);
			System.Type baseAnimValueType = typeof(BaseAnimValue<>);
			while (subclassType != null && subclassType != rawObjectType)
			{
				if (subclassType == baseAnimValueType)
					return true;

				subclassType = subclassType.BaseType != null && subclassType.BaseType.IsGenericType ?
					subclassType.BaseType.GetGenericTypeDefinition() : subclassType.BaseType;
			}

			return false;
		}

		public void Cleanup()
		{
			List<UnityEvent> repaintableEvents = GetRepaintableEvents();
			for (int i = 0; i < repaintableEvents.Count; i++)
			{
				repaintableEvents[i].RemoveListener(Window.ManualRepaint);
			}
			
			if (m_SubEditor != null)
			{
				Object.DestroyImmediate(m_SubEditor);
				m_SubEditor = null;
			}

			if (m_Editor != null)
			{
				if (m_Editor.RequiresConstantRepaint())
				{
					CyoiWindow.RequiresConstantUpdateCounter--;
				}

				Object.DestroyImmediate(m_Editor);
				m_Editor = null;
			}

			Window = null;
		}

		public void OnBeforeSerialize()
		{
			List<UnityEvent> repaintableEvents = GetRepaintableEvents();
			for (int i = 0; i < repaintableEvents.Count; i++)
			{
				repaintableEvents[i].RemoveListener(Window.ManualRepaint);
			}
		}

		public void OnAfterDeserialize()
		{
			EditorApplication.delayCall +=
				delegate()
				{
					if (m_Editor == null)
						return;

					if (m_Editor.target is AssetImporter && m_SubEditor != null)
					{
						System.Type editorType = m_Editor.GetType();

						AssetImporter importer = (AssetImporter)m_Editor.target;
						Object imported = AssetDatabase.LoadAssetAtPath(importer.assetPath, typeof(Object));
						if (imported != null)
						{
							MethodInfo setImportEditor = editorType.GetMethod("InternalSetAssetImporterTargetEditor", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
							if (setImportEditor != null)
							{
								setImportEditor?.Invoke(m_Editor, new object[] { m_SubEditor });
								m_DrawSubEditor = (m_Editor as AssetImporterEditor).showImportedObject;
							}
						}
					}

					//NOTE: don't use EditorTitle here
					List<UnityEvent> repaintableEvents = GetRepaintableEvents();
					for (int i = 0; i < repaintableEvents.Count; i++)
					{
						repaintableEvents[i].AddListener(Window.ManualRepaint);
					}
				};
		}
	}
}
