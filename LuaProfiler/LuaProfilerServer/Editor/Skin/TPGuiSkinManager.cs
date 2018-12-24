/*
* ==============================================================================
* Filename: TPGuiSkinManager
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/

using System;
using UnityEditor;
using UnityEngine;

namespace MikuLuaProfiler
{
    internal static class TPGuiSkinManager
    {
        public class TPGuiStyleManager
        {
            public GUIStyle entryOdd = "OL EntryBackOdd";

            public GUIStyle entryEven = "OL EntryBackEven";

            public GUIStyle numberLabel = "OL Label";

            public GUIStyle searchField = "SearchTextField";

            public GUIStyle SearchCancelButton = "SearchCancelButton";

            public GUIStyle SearchCancelButtonEmpty = "SearchCancelButtonEmpty";

            public GUIStyle boldLabel;

            public GUIStyle largeNumberLabel;

            public GUIStyle largeNumberLabel12;

            public GUIStyle linkLabel;

            public GUIStyle wrapLabel;

            public GUIStyle background;

            public GUIStyle foldout;

            public GUIStyle smallHeader;

            public GUIStyle smallLabel;

            public GUIStyle largeHeader1;

            public GUIStyle largeHeader2;

            public GUIStyle mode;

            public GUIStyle buttonLeft;

            public GUIStyle buttonRight;

            public GUIStyle device;

            public GUIStyle textField;

            public GUIStyle sectionScrollView;

            public GUIStyle settingsBoxTitle;

            public GUIStyle settingsBox;

            public GUIStyle errorLabel;

            public GUIStyle sectionElement;

            public GUIStyle evenRow;

            public GUIStyle oddRow;

            //public GUIStyle selected;

            public GUIStyle keysElement;

            public GUIStyle sectionHeader;



            public TPGuiStyleManager()
            {
                GUIStyle gUIStyle = new GUIStyle("OL Label");
                gUIStyle.fontStyle = (FontStyle)1;
                this.boldLabel = gUIStyle;
                GUIStyle gUIStyle2 = new GUIStyle("OL Label");
                gUIStyle2.fontSize = 15;
                this.largeNumberLabel = gUIStyle2;
                GUIStyle gUIStyle3 = new GUIStyle("OL Label");
                gUIStyle3.fontSize = 12;
                this.largeNumberLabel12 = gUIStyle3;
                GUIStyle gUIStyle4 = new GUIStyle("OL Label");
                gUIStyle4.richText = true;
                this.linkLabel = gUIStyle4;
                GUIStyle gUIStyle5 = new GUIStyle("OL Label");
                gUIStyle5.wordWrap = true;
                this.wrapLabel = gUIStyle5;
                this.background = "OL Box";
                this.foldout = "IN foldout";
                this.smallHeader = new GUIStyle("OL title");
                GUIStyle gUIStyle6 = new GUIStyle("OL title");
                gUIStyle6.font = TPGuiSkinManager.TPSkin.font;
                gUIStyle6.fontStyle = 0;
                this.smallLabel = gUIStyle6;
                GUIStyle gUIStyle7 = new GUIStyle("OL title");
                gUIStyle7.fontSize = 15;
                gUIStyle7.font = TPGuiSkinManager.TPSkin.font;
                gUIStyle7.fontStyle = 0;
                gUIStyle7.fixedHeight = 0f;
                gUIStyle7.padding = new RectOffset(0, 0, 5, 5);
                gUIStyle7.alignment = (TextAnchor)4;
                this.largeHeader1 = gUIStyle7;
                GUIStyle gUIStyle8 = new GUIStyle("OL title");
                gUIStyle8.fontSize = 15;
                gUIStyle8.font = TPGuiSkinManager.TPSkin.font;
                gUIStyle8.fontStyle = 0;
                gUIStyle8.fixedHeight = 0f;
                gUIStyle8.padding = new RectOffset(7, 0, 5, 5);
                gUIStyle8.alignment = (TextAnchor)3;
                this.largeHeader2 = gUIStyle8;
                GUIStyle gUIStyle9 = new GUIStyle("button");
                gUIStyle9.fontSize = 15;
                gUIStyle9.font = TPGuiSkinManager.TPSkin.font;
                gUIStyle9.fontStyle = 0;
                gUIStyle9.fixedHeight = 0f;
                gUIStyle9.padding = new RectOffset(0, 0, 10, 10);
                gUIStyle9.alignment = (TextAnchor)4;
                this.mode = gUIStyle9;
                GUIStyle gUIStyle10 = new GUIStyle("ButtonLeft");
                gUIStyle10.fontSize = 15;
                gUIStyle10.font = TPGuiSkinManager.TPSkin.font;
                gUIStyle10.fontStyle = 0;
                gUIStyle10.fixedHeight = 0f;
                gUIStyle10.padding = new RectOffset(0, 0, 10, 10);
                gUIStyle10.alignment = (TextAnchor)4;
                this.buttonLeft = gUIStyle10;
                GUIStyle gUIStyle11 = new GUIStyle("ButtonRight");
                gUIStyle11.fontSize = 15;
                gUIStyle11.font = TPGuiSkinManager.TPSkin.font;
                gUIStyle11.fontStyle = 0;
                gUIStyle11.fixedHeight = 0f;
                gUIStyle11.padding = new RectOffset(0, 0, 10, 10);
                gUIStyle11.alignment = (TextAnchor)4;
                this.buttonRight = gUIStyle11;
                GUIStyle gUIStyle12 = new GUIStyle("button");
                gUIStyle12.fontSize = 15;
                gUIStyle12.font = TPGuiSkinManager.TPSkin.font;
                gUIStyle12.fontStyle = 0;
                gUIStyle12.fixedHeight = 0f;
                gUIStyle12.padding = new RectOffset(0, 0, 5, 5);
                gUIStyle12.alignment = (TextAnchor)4;
                this.device = gUIStyle12;
                GUIStyle gUIStyle13 = new GUIStyle("textField");
                gUIStyle13.margin = new RectOffset(0, 0, 0, 0);
                this.textField = gUIStyle13;
                this.sectionScrollView = "PreferencesSectionBox";
                this.settingsBoxTitle = "OL Title";
                this.settingsBox = "OL Box";
                this.errorLabel = "WordWrappedLabel";
                this.sectionElement = "PreferencesSection";
                this.evenRow = "CN EntryBackEven";
                this.oddRow = "CN EntryBackOdd";
                //this.selected = "ServerUpdateChangesetOn";
                this.keysElement = "PreferencesKeysElement";
                this.sectionHeader = new GUIStyle(EditorStyles.largeLabel);
                //base..ctor();
            }
        }

        private static GUISkin _tpSkin = null;

        private static TPGuiSkinManager.TPGuiStyleManager _styles = null;

        public static GUISkin TPSkin
        {
            get
            {
                if (TPGuiSkinManager._tpSkin == null)
                {
                    TPGuiSkinManager._tpSkin = (AssetDatabase.LoadAssetAtPath("Assets/LuaProfilerServer/Editor/Skin/TopFunctionsGUISkin.guiskin", typeof(GUISkin)) as GUISkin);
                }
                return TPGuiSkinManager._tpSkin;
            }
            private set
            {
            }
        }

        public static TPGuiSkinManager.TPGuiStyleManager Styles
        {
            get
            {
                if (TPGuiSkinManager._styles == null)
                {
                    TPGuiSkinManager._styles = new TPGuiSkinManager.TPGuiStyleManager();
                }
                return TPGuiSkinManager._styles;
            }
        }
    }
}
