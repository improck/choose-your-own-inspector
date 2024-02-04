using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace ImpRock.Cyoi
{
	internal static class ResId
	{
		public static readonly int ImageTabIcon;

		public static int[] LogStatements;


		static ResId()
		{
			if (EditorGUIUtility.isProSkin || GraphicAssets.ForceProSkin)
			{
				ImageTabIcon = "tabicon_pro.png".GetHashCode();
			}
			else
			{
				ImageTabIcon = "tabicon.png".GetHashCode();
			}
		}
	}

	internal sealed class CyoiResources
	{
		#region Singleton
		private static CyoiResources s_Instance = null;

		public static CyoiResources Instance { get { if (s_Instance == null) { s_Instance = new CyoiResources(); } return s_Instance; } }


		private CyoiResources() { }

		public static void DestroyInstance()
		{
			if (s_Instance != null)
			{
				s_Instance.CleanUp();
				s_Instance = null;
			}
		}
		#endregion


		private Dictionary<int, string> m_TextResources = new Dictionary<int, string>();
		private Dictionary<int, Texture2D> m_ImageResources = new Dictionary<int, Texture2D>();


		public string GetText(int textId)
		{
			return m_TextResources[textId];
		}

		public Texture2D GetImage(int imageId)
		{
			return m_ImageResources[imageId];
		}

		public void LoadResources()
		{
			//text resources
			// switch (Application.systemLanguage)
			// {
			// case SystemLanguage.English:
				// LoadText("cyoilang_en.txt");
				// break;
			// default:
				// LoadText("cyoilang_en.txt");
				// break;
			// }

			//image resources
			if (EditorGUIUtility.isProSkin || GraphicAssets.ForceProSkin)
			{
				//pro skin
				LoadImage("tabicon_pro.png");
			}
			else
			{
				//free skin
				LoadImage("tabicon.png");
			}
		}

		private void LoadText(string fileName)
		{
			TextAsset langAsset = AssetDatabase.LoadAssetAtPath<TextAsset>($"Packages/com.improck.cyoi/Editor/Lang/{fileName}");
			if (langAsset != null)
			{
				using (StringReader reader = new StringReader(langAsset.text))
				{
					int idHash = 0;
					int equalsPos = 0;
					const string equals = " = ";
					string line = string.Empty;
					string id = string.Empty;
					string text = string.Empty;

					List<int> logIds = new List<int>();

					while ((line = reader.ReadLine()) != null)
					{
						if (line.Length == 0 || line[0] == ';')
							continue;

						equalsPos = line.IndexOf(equals);
						id = line.Substring(0, equalsPos);
						text = line.Substring(equalsPos + 3);

						idHash = id.GetHashCode();

						if (id.StartsWith("log"))
						{
							logIds.Add(idHash);
							text = "CYOI: " + text;
						}

						if (m_TextResources.ContainsKey(idHash))
						{
							m_TextResources[idHash] = text;
						}
						else
						{
							m_TextResources.Add(idHash, text);
						}
					}

					ResId.LogStatements = logIds.ToArray();
				}
			}
		}

		private void LoadImage(string fileName)
		{
			int fileNameHash = fileName.GetHashCode();
			if (!m_ImageResources.ContainsKey(fileNameHash))
			{
				Texture2D imageAsset = AssetDatabase.LoadAssetAtPath<Texture2D>($"Packages/com.improck.cyoi/Editor/Images/{fileName}");
				if (imageAsset != null)
				{
					m_ImageResources.Add(fileNameHash, imageAsset);
				}
			}
		}
		
		private void CleanUp()
		{
			//unload images
			foreach (KeyValuePair<int, Texture2D> image in m_ImageResources)
			{
				Object.DestroyImmediate(image.Value);
			}
		}
	}
}
