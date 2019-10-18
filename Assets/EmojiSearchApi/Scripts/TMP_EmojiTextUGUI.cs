using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TMPro
{
    public class TMP_EmojiTextUGUI : TextMeshProUGUI
    {
        #region Private Fields

        protected bool m_emojiParsingRequired = true;

        #endregion

        #region Properties

        protected internal bool IsInputParsingRequired_Internal
        {
            get
            {
#if UNITY_2018_3_OR_NEWER
                return m_havePropertiesChanged;
#else
                return m_isInputParsingRequired;
#endif
            }
            protected set
            {
#if UNITY_2018_3_OR_NEWER
                m_havePropertiesChanged = value;
#else
                m_isInputParsingRequired = value;
#endif
            }
        }

        #endregion

        #region Emoji Parser Functions

        protected virtual bool ParseInputTextAndEmojiCharSequence()
        {
            m_emojiParsingRequired = false;

            //Only parse when richtext active (we need the <sprite=index> tag)
            if (m_isRichText)
            {
                var v_parsedEmoji = false;
                var v_oldText = m_text;

                /*//Parse Sprite Chars
                ParseInputText();

                //Parse Emojis
                m_text = CharBufferToString(); //Pick the char unicodes to parse the emojis
                v_parsedEmoji = TMP_EmojiSearchEngine.ParseEmojiCharSequence(spriteAsset, ref m_text);

                //Apply parsed Emoji to char buffer
                if (v_parsedEmoji)
                {
                    StringToCharArray(m_text, ref m_char_buffer);
                    SetArraySizes(m_char_buffer);
                }*/

                v_parsedEmoji = TMP_EmojiSearchEngine.ParseEmojiCharSequence(spriteAsset, ref m_text);

                m_emojiParsingRequired = false;
                IsInputParsingRequired_Internal = false;

#if !UNITY_2018_3_OR_NEWER
                m_inputSource = TextInputSources.Text;
#endif
                ParseInputText();

                m_emojiParsingRequired = false;
                IsInputParsingRequired_Internal = false;

                //Debug.Log("ParseInputTextAndEmojiCharSequence");
                //We must revert the original text because we dont want to permanently change the text
                m_text = v_oldText;

                m_isCalculateSizeRequired = true;

                return v_parsedEmoji;
            }

            return false;
        }

        #endregion

        #region Text Overriden Functions

        public override void SetVerticesDirty()
        {
            //In textmeshpro 1.4 the parameter "m_isInputParsingRequired" changed to internal, so, to dont use reflection i changed to "m_havePropertiesChanged" parameter
            if (IsInputParsingRequired_Internal)
            {
                m_emojiParsingRequired = m_isRichText;
            }
            base.SetVerticesDirty();
        }

        public override void Rebuild(CanvasUpdate update)
        {
            if (this == null && enabled && gameObject.activeInHierarchy) return;

            if (m_emojiParsingRequired)
                ParseInputTextAndEmojiCharSequence();

            base.Rebuild(update);
        }

        public override string GetParsedText()
        {
            if (m_emojiParsingRequired)
                ParseInputTextAndEmojiCharSequence();

            return base.GetParsedText();
        }

        public override TMP_TextInfo GetTextInfo(string text)
        {
            TMP_EmojiSearchEngine.ParseEmojiCharSequence(spriteAsset, ref text);
            return base.GetTextInfo(text);
        }

        protected override Vector2 CalculatePreferredValues(float defaultFontSize, Vector2 marginSize, bool ignoreTextAutoSizing)
        {
            if (m_emojiParsingRequired)
                ParseInputTextAndEmojiCharSequence();

            return base.CalculatePreferredValues(defaultFontSize, marginSize, ignoreTextAutoSizing);
        }

        /*public string CharBufferToString()
        {
            System.Text.StringBuilder v_builder = new System.Text.StringBuilder();
            if (m_char_buffer != null)
            {
                for (int i = 0; i < m_char_buffer.Length; i++)
                {
                    var v_intValue = m_char_buffer[i];

                    //Not a surrogate and is valid UTF32 (conditions to use char.ConvertFromUtf32 function)
                    if (v_intValue > 0x000000 && v_intValue < 0x10ffff &&
                                        (v_intValue < 0x00d800 || v_intValue > 0x00dfff))
                    {
                        var v_UTF16Surrogate = char.ConvertFromUtf32(v_intValue);
                        if (!string.IsNullOrEmpty(v_UTF16Surrogate))
                        {
                            //Add chars into cache (we must include the both char paths in fastLookupPath)
                            foreach (var v_surrogateChar in v_UTF16Surrogate)
                            {
                                v_builder.Append(v_surrogateChar);
                            }
                        }
                    }
                    //Simple Append
                    else
                    {
                        v_builder.Append((char)v_intValue);
                    }
                }
            }

            return v_builder.ToString();
        }*/

        #endregion
    }
}
