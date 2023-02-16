// Copyright (c) 2021, 2022 Mark A. Olbert 
// 
// This file is part of ConsoleUtilities.
//
// ConsoleUtilities is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// ConsoleUtilities is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with ConsoleUtilities. If not, see <https://www.gnu.org/licenses/>.

namespace J4JSoftware.J4JMapLibrary;

public class BingMapsCultureCodes : Dictionary<string, string>
{
    private BingMapsCultureCodes()
        : base( StringComparer.OrdinalIgnoreCase )
    {
        Add( "af", "Afrikaans" );
        Add( "am", "Amharic" );
        Add( "ar-sa", "Arabic (Saudi Arabia)" );
        Add( "as", "Assamese" );
        Add( "az-Latn", "Azerbaijani (Latin)" );
        Add( "be", "Belarusian" );
        Add( "bg", "Bulgarian" );
        Add( "bn-BD", "Bangla (Bangladesh)" );
        Add( "bn-IN", "Bangla (India)" );
        Add( "bs", "Bosnian (Latin)" );
        Add( "ca", "Catalan Spanish" );
        Add( "ca-ES-valencia", "Valencian" );
        Add( "cs", "Czech" );
        Add( "cy", "Welsh" );
        Add( "da", "Danish" );
        Add( "de", "German (Germany)" );
        Add( "de-de", "German (Germany)" );
        Add( "el", "Greek" );
        Add( "en-GB", "English (United Kingdom)" );
        Add( "en-US", "English (United States)" );
        Add( "es", "Spanish (Spain)" );
        Add( "es-ES", "Spanish (Spain)" );
        Add( "es-US", "Spanish (United States)" );
        Add( "es-MX", "Spanish (Mexico)" );
        Add( "et", "Estonian" );
        Add( "eu", "Basque" );
        Add( "fa", "Persian" );
        Add( "fi", "Finnish" );
        Add( "fil-Latn", "Filipino" );
        Add( "fr", "French (France)" );
        Add( "fr-FR", "French (France)" );
        Add( "fr-CA", "French (Canada)" );
        Add( "ga", "Irish" );
        Add( "gd-Latn", "Scottish Gaelic" );
        Add( "gl", "Galician" );
        Add( "gu", "Gujarati" );
        Add( "ha-Latn", "Hausa (Latin)" );
        Add( "he", "Hebrew" );
        Add( "hi", "Hindi" );
        Add( "hr", "Croatian" );
        Add( "hu", "Hungarian" );
        Add( "hy", "Armenian" );
        Add( "id", "Indonesian" );
        Add( "ig-Latn", "Igbo" );
        Add( "is", "Icelandic" );
        Add( "it", "Italian (Italy)" );
        Add( "it-it", "Italian (Italy)" );
        Add( "ja", "Japanese" );
        Add( "ka", "Georgian" );
        Add( "kk", "Kazakh" );
        Add( "km", "Khmer" );
        Add( "kn", "Kannada" );
        Add( "ko", "Korean" );
        Add( "kok", "Konkani" );
        Add( "ku-Arab", "Central Kurdish" );
        Add( "ky-Cyrl", "Kyrgyz" );
        Add( "lb", "Luxembourgish" );
        Add( "lt", "Lithuanian" );
        Add( "lv", "Latvian" );
        Add( "mi-Latn", "Maori" );
        Add( "mk", "Macedonian" );
        Add( "ml", "Malayalam" );
        Add( "mn-Cyrl", "Mongolian (Cyrillic)" );
        Add( "mr", "Marathi" );
        Add( "ms", "Malay (Malaysia)" );
        Add( "mt", "Maltese" );
        Add( "nb", "Norwegian (Bokmål)" );
        Add( "ne", "Nepali (Nepal)" );
        Add( "nl", "Dutch (Netherlands)" );
        Add( "nl-BE", "Dutch (Netherlands)" );
        Add( "nn", "Norwegian (Nynorsk)" );
        Add( "nso", "Sesotho sa Leboa" );
        Add( "or", "Odia" );
        Add( "pa", "Punjabi (Gurmukhi)" );
        Add( "pa-Arab", "Punjabi (Arabic)" );
        Add( "pl", "Polish" );
        Add( "prs-Arab", "Dari" );
        Add( "pt-BR", "Portuguese (Brazil)" );
        Add( "pt-PT", "Portuguese (Portugal)" );
        Add( "qut-Latn", "K’iche’" );
        Add( "quz", "Quechua (Peru)" );
        Add( "ro", "Romanian (Romania)" );
        Add( "ru", "Russian" );
        Add( "rw", "Kinyarwanda" );
        Add( "sd-Arab", "Sindhi (Arabic)" );
        Add( "si", "Sinhala" );
        Add( "sk", "Slovak" );
        Add( "sl", "Slovenian" );
        Add( "sq", "Albanian" );
        Add( "sr-Cyrl-BA", "Serbian (Cyrillic, Bosnia and Herzegovina)" );
        Add( "sr-Cyrl-RS", "Serbian (Cyrillic, Serbia)" );
        Add( "sr-Latn-RS", "Serbian (Latin, Serbia)" );
        Add( "sv", "Swedish (Sweden)" );
        Add( "sw", "Kiswahili" );
        Add( "ta", "Tamil" );
        Add( "te", "Telugu" );
        Add( "tg-Cyrl", "Tajik (Cyrillic)" );
        Add( "th", "Thai" );
        Add( "ti", "Tigrinya" );
        Add( "tk-Latn", "Turkmen (Latin)" );
        Add( "tn", "Setswana" );
        Add( "tr", "Turkish" );
        Add( "tt-Cyrl", "Tatar (Cyrillic)" );
        Add( "ug-Arab", "Uyghur" );
        Add( "uk", "Ukrainian" );
        Add( "ur", "Urdu" );
        Add( "uz-Latn", "Uzbek (Latin)" );
        Add( "vi", "Vietnamese" );
        Add( "wo", "Wolof" );
        Add( "xh", "isiXhosa" );
        Add( "yo-Latn", "Yoruba" );
        Add( "zh-Hans", "Chinese (Simplified)" );
        Add( "zh-Hant", "Chinese (Traditional)" );
        Add( "zu", "isiZulu" );
    }

    public static BingMapsCultureCodes Default { get; } = new();
}
