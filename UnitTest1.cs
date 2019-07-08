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
        public static void testValidate<T>(object data)
        {
            var dataString = JsonConvert.SerializeObject(data);
            JsonConvert.DeserializeObject<T>(dataString);

            var parsedObject = JObject.Parse(dataString);
            var gen = new JSchemaGenerator();
            gen.DefaultRequired = Required.Default;
            var mailSchema = gen.Generate(typeof(T));

            parsedObject.Validate(mailSchema);
        }

        [Fact]
        public void Test_正常系()
        {
            testValidate<Mail>(new
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
        public MailSource[] from { get; set; }

    }

    public class MailSource
    {
        [Required]
        [JsonProperty("email")]
        string Email { get; set; }
        [JsonProperty("name")]
        string Name { get; set; }
    }
}
