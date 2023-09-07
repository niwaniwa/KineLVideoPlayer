

using System;
using Kinel.VideoPlayer.Scripts;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Kinel.VideoPlayer.Editor
{
    public abstract class KinelEditorBase : UnityEditor.Editor
    {
        protected internal const string DEBUG_LOG_PREFIX = "[<color=#58ACFA>KineL</color>]";
        protected internal const string DEBUG_ERROR_PREFIX = "[<color=#dc143c>KineL</color>]";

        internal const string HEADER_IMAGE_GUID = "6bc2959ee80eb4d4dbdb46be56f94dfa";
        internal const string GITHUB_IMAGE_GUID = "109a0a0acaeaf4b42a8eae55a2a33bbc";
        internal const string TWITTER_IMAGE_GUID = "a9f4234e988d6ee4b957857c0f6d62c4";
        internal const string DISCORD_IMAGE_GUID = "";

        internal const string GITHUB_URL = "https://github.com/niwaniwa/KineLVideoPlayer";
        internal const string TWITTER_URL = "https://twitter.com/ni_rilana";
        
        private Texture headerTexture;
        
        // From AAChair
        private Texture[] textures;
        private string[] guids;
        private string[] urls;
        private Texture githubTexture;
        private Texture twitterTexture;
        // private Texture discordTexture;
        //
        
        public void OnEnable()
        {
            LoadTextures();
            Debug.Log($"is null {githubTexture == null}");
        }

        public void LoadTextures()
        {
            headerTexture = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(HEADER_IMAGE_GUID), typeof(Texture)) as Texture;
            githubTexture = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(GITHUB_IMAGE_GUID), typeof(Texture)) as Texture;
            twitterTexture = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(TWITTER_IMAGE_GUID), typeof(Texture)) as Texture;
            // discordTexture = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guidDiscordIcon), typeof(Texture)) as Texture;
            textures = new Texture[] { githubTexture, twitterTexture };
            guids = new string[] { GITHUB_IMAGE_GUID, TWITTER_IMAGE_GUID };
            urls = new string[] { GITHUB_URL, TWITTER_URL };
        }

        public override void OnInspectorGUI()
        {
            DrawHeader();
        }

        public void DrawHeader()
        {
            
            EditorGUILayout.Space();
            DrawLogoTexture(HEADER_IMAGE_GUID, headerTexture);
            EditorGUILayout.Space();
            
            EditorGUILayout.Space();
            if (textures == null)
            {
                LoadTextures();
            }
        }

        public void DrawFooter()
        {
            UdonSharpGUI.DrawUILine();
            EditorGUILayout.Space();
            DrawSocialLinks(textures, guids, urls);
            EditorGUILayout.Space();
        }

        /// <summary>
        /// From AAChair by Kamishiro (https://github.com/AoiKamishiro/VRChatPrefabs/blob/master/Assets/00Kamishiro/AAChair/AAChair-README_JP.md) mit license
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="texture"></param>
        private void DrawLogoTexture(string guid, Texture texture)
        {
            if (texture == null)
            {
                texture = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(Texture)) as Texture;
            }
            if (texture != null)
            {
                float w = EditorGUIUtility.currentViewWidth;
                Rect rect = new Rect
                {
                    width = w - 40f
                };
                rect.height = rect.width / 4f;
                Rect rect2 = GUILayoutUtility.GetRect(rect.width, rect.height);
                rect.x = ((EditorGUIUtility.currentViewWidth - rect.width) * 0.5f) - 4.0f;
                rect.y = rect2.y;
                GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill);
            }
        }

        /// <summary>
        /// From AAChair by Kamishiro (https://github.com/AoiKamishiro/VRChatPrefabs/blob/master/Assets/00Kamishiro/AAChair/AAChair-README_JP.md) mit license
        /// </summary>
        /// <param name="textures"></param>
        /// <param name="guids"></param>
        /// <param name="urls"></param>
        private void DrawSocialLinks(Texture[] textures, string[] guids, string[] urls)
        {
            float space = 10f;
            float padding = 10f;
            float size = 40f;

            float w = size * textures.Length + space * (textures.Length - 1);
            Rect socialAreaRect = new Rect
            {
                width = w,
                height = size + padding * 2
            };
            Rect sar = GUILayoutUtility.GetRect(socialAreaRect.width, socialAreaRect.height);
            for (int i = 0; i < textures.Length; i++)
            {
                if (textures[i] == null)
                {
                    textures[i] =
                        AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[i]), typeof(Texture)) as
                            Texture;
                }

                if (textures[i] != null)
                {
                    Rect rect = new Rect
                    {
                        width = size,
                        height = size,
                        x = ((EditorGUIUtility.currentViewWidth - w) * 0.5f) - 4.0f + size * i + space * i,
                        y = sar.y + padding
                    };
                    GUI.DrawTexture(rect, textures[i], ScaleMode.StretchToFill);
                    if (GUI.Button(rect, "", new GUIStyle()))
                    {
                        Application.OpenURL(urls[i]);
                    }
                }
            }
        }
        
        public KinelVideoPlayerScript[] GetVideoPlayers()
        {
            return FindObjectsOfType<KinelVideoPlayerScript>();
        }

        public abstract void ApplyUdonProperties();
        


    }
}