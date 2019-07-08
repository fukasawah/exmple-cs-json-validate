using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using Xunit;

namespace cs_json_validate_reader_example
{
    public class UnitTest1
    {
        public static T testValidate<T>(object data)
        {
            var dataString = JsonConvert.SerializeObject(data);

            var parsedObject = JObject.Parse(dataString);
            var gen = new JSchemaGenerator();
            gen.DefaultRequired = Required.Default;
            var mailSchema = gen.Generate(typeof(T));

            parsedObject.Validate(mailSchema);

            return JsonConvert.DeserializeObject<T>(dataString);
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
                }
            });

            Assert.Equal("rd@dummy.example", mail.To.Email);
            Assert.Equal("RD", mail.To.Name);

            Assert.Equal("fukasawah@dummy.example", mail.From[0].Email);
            Assert.Equal("fukasawah", mail.From[0].Name);

        }


        [Fact]
        public void Test_Toがないならばエラー()
        {
            Assert.Throws(typeof(JSchemaValidationException), () => testValidate<Mail>(new
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
            Assert.Throws(typeof(JSchemaValidationException), () => testValidate<Mail>(new
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
            Assert.Throws(typeof(JSchemaValidationException), () => testValidate<Mail>(new
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
    }

    public class MailSource
    {
        [Required]
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
