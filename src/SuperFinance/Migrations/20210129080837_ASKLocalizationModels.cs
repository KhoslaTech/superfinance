using System;
using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ASKSource.Migrations
{
	public partial class ASKLocalizationModels : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<string>(
				name: "CultureCode",
				table: "User",
				maxLength: 20,
				nullable: false,
				defaultValue: "");

			migrationBuilder.AddColumn<int>(
				name: "TimeZoneId",
				table: "User",
				nullable: false,
				defaultValue: 0);

			migrationBuilder.CreateTable(
				name: "Culture",
				columns: table => new
				{
					CultureCode = table.Column<string>(maxLength: 20, nullable: false),
					DisplayName = table.Column<string>(maxLength: 75, nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Culture", x => x.CultureCode);
				});

			migrationBuilder.CreateTable(
				name: "TimeZone",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					TimeZoneCode = table.Column<string>(maxLength: 60, nullable: false),
					DisplayName = table.Column<string>(maxLength: 100, nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_TimeZone", x => x.Id);
				});

			migrationBuilder.CreateIndex(
				name: "IX_User_CultureCode",
				table: "User",
				column: "CultureCode");

			migrationBuilder.CreateIndex(
				name: "IX_User_TimeZoneId",
				table: "User",
				column: "TimeZoneId");

			migrationBuilder.CreateIndex(
				name: "IX_TimeZone_TimeZoneCode",
				table: "TimeZone",
				column: "TimeZoneCode",
				unique: true);

			migrationBuilder.AddForeignKey(
				name: "FK_User_Culture_CultureCode",
				table: "User",
				column: "CultureCode",
				principalTable: "Culture",
				principalColumn: "CultureCode");

			migrationBuilder.AddForeignKey(
				name: "FK_User_TimeZone_TimeZoneId",
				table: "User",
				column: "TimeZoneId",
				principalTable: "TimeZone",
				principalColumn: "Id");

			InsertTimeZones(migrationBuilder);
			InsertCultures(migrationBuilder);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "FK_User_Culture_CultureCode",
				table: "User");

			migrationBuilder.DropForeignKey(
				name: "FK_User_TimeZone_TimeZoneId",
				table: "User");

			migrationBuilder.DropTable(
				name: "Culture");

			migrationBuilder.DropTable(
				name: "TimeZone");

			migrationBuilder.DropIndex(
				name: "IX_User_CultureCode",
				table: "User");

			migrationBuilder.DropIndex(
				name: "IX_User_TimeZoneId",
				table: "User");

			migrationBuilder.DropColumn(
				name: "CultureCode",
				table: "User");

			migrationBuilder.DropColumn(
				name: "TimeZoneId",
				table: "User");
		}

		private void InsertTimeZones(MigrationBuilder migrationBuilder)
		{
			try
			{
				var valuesBuilder = new StringBuilder();
				var timeZones = TimeZoneInfo.GetSystemTimeZones();

				foreach (var zone in timeZones)
				{
					var displayName = zone.DisplayName.Contains("'")
						? zone.DisplayName.Replace("'", "''")
						: zone.DisplayName;

					var value = valuesBuilder.Length == 0
						? $"('{zone.Id}',N'{displayName}')"
						: $",('{zone.Id}',N'{displayName}')";

					valuesBuilder.Append(value);
				}

				var sql = $"insert into [dbo].[TimeZone] (TimeZoneCode, DisplayName) values {valuesBuilder}";
				migrationBuilder.Sql(sql);
			}
			catch
			{
				migrationBuilder.Sql(@"insert into [dbo].[TimeZone]
				(TimeZoneCode, DisplayName)
				values('Afghanistan Standard Time', '(UTC+04:30) Kabul'),
				('Alaskan Standard Time', '(UTC-09:00) Alaska'),
				('Arab Standard Time', '(UTC+03:00) Kuwait, Riyadh'),
				('Arabian Standard Time', '(UTC+04:00) Abu Dhabi, Muscat'),
				('Arabic Standard Time', '(UTC+03:00) Baghdad'),
				('Argentina Standard Time', '(UTC-03:00) Buenos Aires'),
				('Atlantic Standard Time', '(UTC-04:00) Atlantic Time (Canada)'),
				('AUS Central Standard Time', '(UTC+09:30) Darwin'),
				('AUS Eastern Standard Time', '(UTC+10:00) Canberra, Melbourne, Sydney'),
				('Azerbaijan Standard Time', '(UTC+04:00) Baku'),
				('Azores Standard Time', '(UTC-01:00) Azores'),
				('Bahia Standard Time', '(UTC-03:00) Salvador'),
				('Bangladesh Standard Time', '(UTC+06:00) Dhaka'),
				('Canada Central Standard Time', '(UTC-06:00) Saskatchewan'),
				('Cape Verde Standard Time', '(UTC-01:00) Cape Verde Is.'),
				('Caucasus Standard Time', '(UTC+04:00) Yerevan'),
				('Cen. Australia Standard Time', '(UTC+09:30) Adelaide'),
				('Central America Standard Time', '(UTC-06:00) Central America'),
				('Central Asia Standard Time', '(UTC+06:00) Astana'),
				('Central Brazilian Standard Time', '(UTC-04:00) Cuiaba'),
				('Central Europe Standard Time', '(UTC+01:00) Belgrade, Bratislava, Budapest, Ljubljana, Prague'),
				('Central European Standard Time', '(UTC+01:00) Sarajevo, Skopje, Warsaw, Zagreb'),
				('Central Pacific Standard Time', '(UTC+11:00) Solomon Is., New Caledonia'),
				('Central Standard Time', '(UTC-06:00) Central Time (US & Canada)'),
				('Central Standard Time (Mexico)', '(UTC-06:00) Guadalajara, Mexico City, Monterrey'),
				('China Standard Time', '(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi'),
				('Dateline Standard Time', '(UTC-12:00) International Date Line West'),
				('E. Africa Standard Time', '(UTC+03:00) Nairobi'),
				('E. Australia Standard Time', '(UTC+10:00) Brisbane'),
				('E. Europe Standard Time', '(UTC+02:00) E. Europe'),
				('E. South America Standard Time', '(UTC-03:00) Brasilia'),
				('Eastern Standard Time', '(UTC-05:00) Eastern Time (US & Canada)'),
				('Egypt Standard Time', '(UTC+02:00) Cairo'),
				('Ekaterinburg Standard Time', '(UTC+06:00) Ekaterinburg'),
				('Fiji Standard Time', '(UTC+12:00) Fiji'),
				('FLE Standard Time', '(UTC+02:00) Helsinki, Kyiv, Riga, Sofia, Tallinn, Vilnius'),
				('Georgian Standard Time', '(UTC+04:00) Tbilisi'),
				('GMT Standard Time', '(UTC) Dublin, Edinburgh, Lisbon, London'),
				('Greenland Standard Time', '(UTC-03:00) Greenland'),
				('Greenwich Standard Time', '(UTC) Monrovia, Reykjavik'),
				('GTB Standard Time', '(UTC+02:00) Athens, Bucharest'),
				('Hawaiian Standard Time', '(UTC-10:00) Hawaii'),
				('India Standard Time', '(UTC+05:30) Chennai, Kolkata, Mumbai, New Delhi'),
				('Iran Standard Time', '(UTC+03:30) Tehran'),
				('Israel Standard Time', '(UTC+02:00) Jerusalem'),
				('Jordan Standard Time', '(UTC+03:00) Amman'),
				('Kaliningrad Standard Time', '(UTC+03:00) Kaliningrad, Minsk'),
				('Kamchatka Standard Time', '(UTC+12:00) Petropavlovsk-Kamchatsky - Old'),
				('Korea Standard Time', '(UTC+09:00) Seoul'),
				('Libya Standard Time', '(UTC+01:00) Tripoli'),
				('Magadan Standard Time', '(UTC+12:00) Magadan'),
				('Mauritius Standard Time', '(UTC+04:00) Port Louis'),
				('Mid-Atlantic Standard Time', '(UTC-02:00) Mid-Atlantic'),
				('Middle East Standard Time', '(UTC+02:00) Beirut'),
				('Montevideo Standard Time', '(UTC-03:00) Montevideo'),
				('Morocco Standard Time', '(UTC) Casablanca'),
				('Mountain Standard Time', '(UTC-07:00) Mountain Time (US & Canada)'),
				('Mountain Standard Time (Mexico)', '(UTC-07:00) Chihuahua, La Paz, Mazatlan'),
				('Myanmar Standard Time', '(UTC+06:30) Yangon (Rangoon)'),
				('N. Central Asia Standard Time', '(UTC+07:00) Novosibirsk'),
				('Namibia Standard Time', '(UTC+01:00) Windhoek'),
				('Nepal Standard Time', '(UTC+05:45) Kathmandu'),
				('New Zealand Standard Time', '(UTC+12:00) Auckland, Wellington'),
				('Newfoundland Standard Time', '(UTC-03:30) Newfoundland'),
				('North Asia East Standard Time', '(UTC+09:00) Irkutsk'),
				('North Asia Standard Time', '(UTC+08:00) Krasnoyarsk'),
				('Pacific SA Standard Time', '(UTC-04:00) Santiago'),
				('Pacific Standard Time', '(UTC-08:00) Pacific Time (US & Canada)'),
				('Pacific Standard Time (Mexico)', '(UTC-08:00) Baja California'),
				('Pakistan Standard Time', '(UTC+05:00) Islamabad, Karachi'),
				('Paraguay Standard Time', '(UTC-04:00) Asuncion'),
				('Romance Standard Time', '(UTC+01:00) Brussels, Copenhagen, Madrid, Paris'),
				('Russian Standard Time', '(UTC+04:00) Moscow, St. Petersburg, Volgograd'),
				('SA Eastern Standard Time', '(UTC-03:00) Cayenne, Fortaleza'),
				('SA Pacific Standard Time', '(UTC-05:00) Bogota, Lima, Quito'),
				('SA Western Standard Time', '(UTC-04:00) Georgetown, La Paz, Manaus, San Juan'),
				('Samoa Standard Time', '(UTC+13:00) Samoa'),
				('SE Asia Standard Time', '(UTC+07:00) Bangkok, Hanoi, Jakarta'),
				('Singapore Standard Time', '(UTC+08:00) Kuala Lumpur, Singapore'),
				('South Africa Standard Time', '(UTC+02:00) Harare, Pretoria'),
				('Sri Lanka Standard Time', '(UTC+05:30) Sri Jayawardenepura'),
				('Syria Standard Time', '(UTC+02:00) Damascus'),
				('Taipei Standard Time', '(UTC+08:00) Taipei'),
				('Tasmania Standard Time', '(UTC+10:00) Hobart'),
				('Tokyo Standard Time', '(UTC+09:00) Osaka, Sapporo, Tokyo'),
				('Tonga Standard Time', '(UTC+13:00) Nuku''alofa'),
				('Turkey Standard Time', '(UTC+02:00) Istanbul'),
				('Ulaanbaatar Standard Time', '(UTC+08:00) Ulaanbaatar'),
				('US Eastern Standard Time', '(UTC-05:00) Indiana (East)'),
				('US Mountain Standard Time', '(UTC-07:00) Arizona'),
				('UTC', '(UTC) Coordinated Universal Time'),
				('UTC+12', '(UTC+12:00) Coordinated Universal Time+12'),
				('UTC-02', '(UTC-02:00) Coordinated Universal Time-02'),
				('UTC-11', '(UTC-11:00) Coordinated Universal Time-11'),
				('Venezuela Standard Time', '(UTC-04:30) Caracas'),
				('Vladivostok Standard Time', '(UTC+11:00) Vladivostok'),
				('W. Australia Standard Time', '(UTC+08:00) Perth'),
				('W. Central Africa Standard Time', '(UTC+01:00) West Central Africa'),
				('W. Europe Standard Time', '(UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna'),
				('West Asia Standard Time', '(UTC+05:00) Tashkent'),
				('West Pacific Standard Time', '(UTC+10:00) Guam, Port Moresby'),
				('Yakutsk Standard Time', '(UTC+10:00) Yakutsk')
				");
			}
		}

		private void InsertCultures(MigrationBuilder migrationBuilder)
		{
			try
			{
				var valuesBuilder = new StringBuilder();
				var cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);

				foreach (var culture in cultures)
				{
					var displayName = culture.EnglishName.Contains("'")
						? culture.EnglishName.Replace("'", "''")
						: culture.EnglishName;

					var value = valuesBuilder.Length == 0
						? $"('{culture.Name}',N'{displayName}')"
						: $",('{culture.Name}',N'{displayName}')";

					valuesBuilder.Append(value);
				}

				var sql = $"insert into [dbo].[Culture] (CultureCode, DisplayName) values {valuesBuilder}";
				migrationBuilder.Sql(sql);
			}
			catch
			{
				migrationBuilder.Sql(@"insert into [dbo].[Culture]
				(CultureCode, DisplayName)
				values('ar-SA', 'Arabic (Saudi Arabia)'),
				('bg-BG', 'Bulgarian (Bulgaria)'),
				('ca-ES', 'Catalan (Catalan)'),
				('zh-TW', 'Chinese (Traditional, Taiwan)'),
				('cs-CZ', 'Czech (Czech Republic)'),
				('da-DK', 'Danish (Denmark)'),
				('de-DE', 'German (Germany)'),
				('el-GR', 'Greek (Greece)'),
				('en-US', 'English (United States)'),
				('fi-FI', 'Finnish (Finland)'),
				('fr-FR', 'French (France)'),
				('he-IL', 'Hebrew (Israel)'),
				('hu-HU', 'Hungarian (Hungary)'),
				('is-IS', 'Icelandic (Iceland)'),
				('it-IT', 'Italian (Italy)'),
				('ja-JP', 'Japanese (Japan)'),
				('ko-KR', 'Korean (Korea)'),
				('nl-NL', 'Dutch (Netherlands)'),
				('nb-NO', 'Norwegian, Bokmål (Norway)'),
				('pl-PL', 'Polish (Poland)'),
				('pt-BR', 'Portuguese (Brazil)'),
				('rm-CH', 'Romansh (Switzerland)'),
				('ro-RO', 'Romanian (Romania)'),
				('ru-RU', 'Russian (Russia)'),
				('hr-HR', 'Croatian (Croatia)'),
				('sk-SK', 'Slovak (Slovakia)'),
				('sq-AL', 'Albanian (Albania)'),
				('sv-SE', 'Swedish (Sweden)'),
				('th-TH', 'Thai (Thailand)'),
				('tr-TR', 'Turkish (Turkey)'),
				('ur-PK', 'Urdu (Islamic Republic of Pakistan)'),
				('id-ID', 'Indonesian (Indonesia)'),
				('uk-UA', 'Ukrainian (Ukraine)'),
				('be-BY', 'Belarusian (Belarus)'),
				('sl-SI', 'Slovenian (Slovenia)'),
				('et-EE', 'Estonian (Estonia)'),
				('lv-LV', 'Latvian (Latvia)'),
				('lt-LT', 'Lithuanian (Lithuania)'),
				('tg-Cyrl-TJ', 'Tajik (Cyrillic, Tajikistan)'),
				('fa-IR', 'Persian'),
				('vi-VN', 'Vietnamese (Vietnam)'),
				('hy-AM', 'Armenian (Armenia)'),
				('az-Latn-AZ', 'Azeri (Latin, Azerbaijan)'),
				('eu-ES', 'Basque (Basque)'),
				('hsb-DE', 'Upper Sorbian (Germany)'),
				('mk-MK', 'Macedonian (Former Yugoslav Republic of Macedonia)'),
				('tn-ZA', 'Setswana (South Africa)'),
				('xh-ZA', 'isiXhosa (South Africa)'),
				('zu-ZA', 'isiZulu (South Africa)'),
				('af-ZA', 'Afrikaans (South Africa)'),
				('ka-GE', 'Georgian (Georgia)'),
				('fo-FO', 'Faroese (Faroe Islands)'),
				('hi-IN', 'Hindi (India)'),
				('mt-MT', 'Maltese (Malta)'),
				('se-NO', 'Sami, Northern (Norway)'),
				('ms-MY', 'Malay (Malaysia)'),
				('kk-KZ', 'Kazakh (Kazakhstan)'),
				('ky-KG', 'Kyrgyz (Kyrgyzstan)'),
				('sw-KE', 'Kiswahili (Kenya)'),
				('tk-TM', 'Turkmen (Turkmenistan)'),
				('uz-Latn-UZ', 'Uzbek (Latin, Uzbekistan)'),
				('tt-RU', 'Tatar (Russia)'),
				('bn-IN', 'Bengali (India)'),
				('pa-IN', 'Punjabi (India)'),
				('gu-IN', 'Gujarati (India)'),
				('or-IN', 'Oriya (India)'),
				('ta-IN', 'Tamil (India)'),
				('te-IN', 'Telugu (India)'),
				('kn-IN', 'Kannada (India)'),
				('ml-IN', 'Malayalam (India)'),
				('as-IN', 'Assamese (India)'),
				('mr-IN', 'Marathi (India)'),
				('sa-IN', 'Sanskrit (India)'),
				('mn-MN', 'Mongolian (Cyrillic, Mongolia)'),
				('bo-CN', 'Tibetan (PRC)'),
				('cy-GB', 'Welsh (United Kingdom)'),
				('km-KH', 'Khmer (Cambodia)'),
				('lo-LA', 'Lao (Lao P.D.R.)'),
				('gl-ES', 'Galician (Galician)'),
				('kok-IN', 'Konkani (India)'),
				('syr-SY', 'Syriac (Syria)'),
				('si-LK', 'Sinhala (Sri Lanka)'),
				('iu-Cans-CA', 'Inuktitut (Syllabics, Canada)'),
				('am-ET', 'Amharic (Ethiopia)'),
				('ne-NP', 'Nepali (Nepal)'),
				('fy-NL', 'Frisian (Netherlands)'),
				('ps-AF', 'Pashto (Afghanistan)'),
				('fil-PH', 'Filipino (Philippines)'),
				('dv-MV', 'Divehi (Maldives)'),
				('ha-Latn-NG', 'Hausa (Latin, Nigeria)'),
				('yo-NG', 'Yoruba (Nigeria)'),
				('quz-BO', 'Quechua (Bolivia)'),
				('nso-ZA', 'Sesotho sa Leboa (South Africa)'),
				('ba-RU', 'Bashkir (Russia)'),
				('lb-LU', 'Luxembourgish (Luxembourg)'),
				('kl-GL', 'Greenlandic (Greenland)'),
				('ig-NG', 'Igbo (Nigeria)'),
				('ii-CN', 'Yi (PRC)'),
				('arn-CL', 'Mapudungun (Chile)'),
				('moh-CA', 'Mohawk (Mohawk)'),
				('br-FR', 'Breton (France)'),
				('ug-CN', 'Uyghur (PRC)'),
				('mi-NZ', 'Maori (New Zealand)'),
				('oc-FR', 'Occitan (France)'),
				('co-FR', 'Corsican (France)'),
				('gsw-FR', 'Alsatian (France)'),
				('sah-RU', 'Sakha (Russia)'),
				('qut-GT', 'K''iche (Guatemala)'),
				('rw-RW', 'Kinyarwanda (Rwanda)'),
				('wo-SN', 'Wolof (Senegal)'),
				('prs-AF', 'Dari (Afghanistan)'),
				('gd-GB', 'Scottish Gaelic (United Kingdom)'),
				('ar-IQ', 'Arabic (Iraq)'),
				('zh-CN', 'Chinese (Simplified, PRC)'),
				('de-CH', 'German (Switzerland)'),
				('en-GB', 'English (United Kingdom)'),
				('es-MX', 'Spanish (Mexico)'),
				('fr-BE', 'French (Belgium)'),
				('it-CH', 'Italian (Switzerland)'),
				('nl-BE', 'Dutch (Belgium)'),
				('nn-NO', 'Norwegian, Nynorsk (Norway)'),
				('pt-PT', 'Portuguese (Portugal)'),
				('sr-Latn-CS', 'Serbian (Latin, Serbia and Montenegro (Former))'),
				('sv-FI', 'Swedish (Finland)'),
				('az-Cyrl-AZ', 'Azeri (Cyrillic, Azerbaijan)'),
				('dsb-DE', 'Lower Sorbian (Germany)'),
				('se-SE', 'Sami, Northern (Sweden)'),
				('ga-IE', 'Irish (Ireland)'),
				('ms-BN', 'Malay (Brunei Darussalam)'),
				('uz-Cyrl-UZ', 'Uzbek (Cyrillic, Uzbekistan)'),
				('bn-BD', 'Bengali (Bangladesh)'),
				('mn-Mong-CN', 'Mongolian (Traditional Mongolian, PRC)'),
				('iu-Latn-CA', 'Inuktitut (Latin, Canada)'),
				('tzm-Latn-DZ', 'Tamazight (Latin, Algeria)'),
				('quz-EC', 'Quechua (Ecuador)'),
				('ar-EG', 'Arabic (Egypt)'),
				('zh-HK', 'Chinese (Traditional, Hong Kong S.A.R.)'),
				('de-AT', 'German (Austria)'),
				('en-AU', 'English (Australia)'),
				('es-ES', 'Spanish (Spain)'),
				('fr-CA', 'French (Canada)'),
				('sr-Cyrl-CS', 'Serbian (Cyrillic, Serbia and Montenegro (Former))'),
				('se-FI', 'Sami, Northern (Finland)'),
				('quz-PE', 'Quechua (Peru)'),
				('ar-LY', 'Arabic (Libya)'),
				('zh-SG', 'Chinese (Simplified, Singapore)'),
				('de-LU', 'German (Luxembourg)'),
				('en-CA', 'English (Canada)'),
				('es-GT', 'Spanish (Guatemala)'),
				('fr-CH', 'French (Switzerland)'),
				('hr-BA', 'Croatian (Latin, Bosnia and Herzegovina)'),
				('smj-NO', 'Sami, Lule (Norway)'),
				('ar-DZ', 'Arabic (Algeria)'),
				('zh-MO', 'Chinese (Traditional, Macao S.A.R.)'),
				('de-LI', 'German (Liechtenstein)'),
				('en-NZ', 'English (New Zealand)'),
				('es-CR', 'Spanish (Costa Rica)'),
				('fr-LU', 'French (Luxembourg)'),
				('bs-Latn-BA', 'Bosnian (Latin, Bosnia and Herzegovina)'),
				('smj-SE', 'Sami, Lule (Sweden)'),
				('ar-MA', 'Arabic (Morocco)'),
				('en-IE', 'English (Ireland)'),
				('es-PA', 'Spanish (Panama)'),
				('fr-MC', 'French (Monaco)'),
				('sr-Latn-BA', 'Serbian (Latin, Bosnia and Herzegovina)'),
				('sma-NO', 'Sami, Southern (Norway)'),
				('ar-TN', 'Arabic (Tunisia)'),
				('en-ZA', 'English (South Africa)'),
				('es-DO', 'Spanish (Dominican Republic)'),
				('sr-Cyrl-BA', 'Serbian (Cyrillic, Bosnia and Herzegovina)'),
				('sma-SE', 'Sami, Southern (Sweden)'),
				('ar-OM', 'Arabic (Oman)'),
				('en-JM', 'English (Jamaica)'),
				('es-VE', 'Spanish (Bolivarian Republic of Venezuela)'),
				('bs-Cyrl-BA', 'Bosnian (Cyrillic, Bosnia and Herzegovina)'),
				('sms-FI', 'Sami, Skolt (Finland)'),
				('ar-YE', 'Arabic (Yemen)'),
				('en-029', 'English (Caribbean)'),
				('es-CO', 'Spanish (Colombia)'),
				('sr-Latn-RS', 'Serbian (Latin, Serbia)'),
				('smn-FI', 'Sami, Inari (Finland)'),
				('ar-SY', 'Arabic (Syria)'),
				('en-BZ', 'English (Belize)'),
				('es-PE', 'Spanish (Peru)'),
				('sr-Cyrl-RS', 'Serbian (Cyrillic, Serbia)'),
				('ar-JO', 'Arabic (Jordan)'),
				('en-TT', 'English (Trinidad and Tobago)'),
				('es-AR', 'Spanish (Argentina)'),
				('sr-Latn-ME', 'Serbian (Latin, Montenegro)'),
				('ar-LB', 'Arabic (Lebanon)'),
				('en-ZW', 'English (Zimbabwe)'),
				('es-EC', 'Spanish (Ecuador)'),
				('sr-Cyrl-ME', 'Serbian (Cyrillic, Montenegro)'),
				('ar-KW', 'Arabic (Kuwait)'),
				('en-PH', 'English (Republic of the Philippines)'),
				('es-CL', 'Spanish (Chile)'),
				('ar-AE', 'Arabic (U.A.E.)'),
				('es-UY', 'Spanish (Uruguay)'),
				('ar-BH', 'Arabic (Bahrain)'),
				('es-PY', 'Spanish (Paraguay)'),
				('ar-QA', 'Arabic (Qatar)'),
				('en-IN', 'English (India)'),
				('es-BO', 'Spanish (Bolivia)'),
				('en-MY', 'English (Malaysia)'),
				('es-SV', 'Spanish (El Salvador)'),
				('en-SG', 'English (Singapore)'),
				('es-HN', 'Spanish (Honduras)'),
				('es-NI', 'Spanish (Nicaragua)'),
				('es-PR', 'Spanish (Puerto Rico)'),
				('es-US', 'Spanish (United States)')
				");
			}
		}
	}
}
