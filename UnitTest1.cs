using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using Xunit;

namespace exmple_cs_json_validate
{
    public class UnitTest1
    {
        /// <summary>
        /// 型引数からJSchemaを生成します。
        /// </summary>
        public static JSchema generateSchema<T>()
        {
            var gen = new JSchemaGenerator();
            gen.DefaultRequired = Required.DisallowNull;
            return gen.Generate(typeof(T));
        }

        /// <summary>
        /// （テスト用）匿名型などからJObjectへ変換します
        /// </summary>
        public static JObject parse(object data)
        {
            var dataString = JsonConvert.SerializeObject(data);
            return JObject.Parse(dataString);
        }

        public static T testValidate<T>(object data)
        {

            var mailSchema = generateSchema<T>();
            var parsedObject = parse(data);

            parsedObject.Validate(mailSchema);

            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(data));
        }

        [Fact]
        public void Test_正常系()
        {
            var mail = testValidate<Mail>(new
            {
                to = new
                {
                    email = "rd@dummy.example",
                    name = "RD",
                },
                from = new[] {
                    new {
                        email = "fukasawah@dummy.example",
                        name = "fukasawah"
                    }
                },
                cc = new[] {
                    new {
                        email = "cc-fukasawah-1@dummy.example",
                        name = "cc-fukasawah-1"
                    }
                },
                bcc = new[] {
                    new {
                        email = "bcc-fukasawah-1@dummy.example",
                        name = "bcc-fukasawah-1"
                    }
                }
            });

            Assert.Equal("rd@dummy.example", mail.To.Email);
            Assert.Equal("RD", mail.To.Name);

            Assert.Equal("fukasawah@dummy.example", mail.From[0].Email);
            Assert.Equal("fukasawah", mail.From[0].Name);

            Assert.Equal("cc-fukasawah-1@dummy.example", mail.Cc[0].Email);
            Assert.Equal("cc-fukasawah-1", mail.Cc[0].Name);

            Assert.Equal("bcc-fukasawah-1@dummy.example", mail.Bcc[0].Email);
            Assert.Equal("bcc-fukasawah-1", mail.Bcc[0].Name);


        }


        [Fact]
        public void Test_ValidationErrorの確認()
        {
            var mailSchema = generateSchema<Mail>();
            var parsedObject = parse(new
            {
                cc = new[] {
                    new {
                        // CCのEmailフォーマット不正
                        email = "foo@bar@baz",
                        name = "cc-fukasawah-1"
                    }
                },
                bcc = new[] {
                    new {
                        // BCCのEmailない
                        name = "bcc-fukasawah-1"
                    }
                }
                // Toない
                // Fromない
            });

            parsedObject.IsValid(mailSchema, out IList<ValidationError> errors);

            Assert.Equal(3, errors.Count);

            var i = 0;
            Assert.Equal("cc[0].email", errors[i].Path);
            Assert.Equal(ErrorType.Pattern, errors[i].ErrorType);
            Assert.Equal("foo@bar@baz", errors[i].Value);

            i++;
            Assert.Equal("bcc[0]", errors[i].Path);
            Assert.Equal(ErrorType.Required, errors[i].ErrorType);
            Assert.Equal(new[] { "email" }, errors[i].Value);

            i++;
            Assert.Equal("", errors[i].Path); // ROOT
            Assert.Equal(ErrorType.Required, errors[i].ErrorType);
            Assert.Contains("to", errors[i].Value as IList<string>);
            Assert.Contains("from", errors[i].Value as IList<string>);
        }

        [Fact]
        public void Test_Toがないならばエラー()
        {
            Assert.Throws<JSchemaValidationException>(() => testValidate<Mail>(new
            {
                from = new[] {
                    new {
                        email = "fukasawah@dummy.example",
                        name = "fukasawah"
                    }
                }
            }));
        }
        [Fact]
        public void Test_Fromがないならばエラー()
        {
            Assert.Throws<JSchemaValidationException>(() => testValidate<Mail>(new
            {
                to = new
                {
                    email = "rd@dummy.example",
                    name = "RD",
                }
            }));
        }
        [Fact]
        public void Test_Fromが長さ0ならばエラー()
        {
            Assert.Throws<JSchemaValidationException>(() => testValidate<Mail>(new
            {
                to = new
                {
                    email = "rd@dummy.example",
                    name = "RD",
                },
                from = new object[0]
            }));
        }

    }

    public class Mail
    {
        [Required]
        [JsonProperty("to")]
        public MailSource To { get; set; }

        [Required]
        [MinLength(1)]
        [JsonProperty("from")]
        public MailSource[] From { get; set; }

        [JsonProperty("cc")]
        public MailSource[] Cc { get; set; }
        [JsonProperty("bcc")]
        public MailSource[] Bcc { get; set; }

    }

    public class MailSource
    {
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9.!#$%&'*+\/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$")] // ref: https://www.w3.org/TR/html5/forms.html#valid-e-mail-address
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
