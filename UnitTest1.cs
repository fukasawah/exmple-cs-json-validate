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
        [Fact]
        public void Test_正常系()
        {
            var gen = new JSchemaGenerator();
            gen.DefaultRequired = Required.Default;

            var mailSchema = gen.Generate(typeof(Mail));

            var data = JsonConvert.SerializeObject(new
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

            JObject mailObject = JObject.Parse(data);
            mailObject.Validate(mailSchema);
        }

        [Fact]
        public void Test_Toがないとエラー()
        {
            var gen = new JSchemaGenerator();
            gen.DefaultRequired = Required.Default;

            var mailSchema = gen.Generate(typeof(Mail));

            var data = JsonConvert.SerializeObject(new
            {
                from = new[] {
                    new {
                        email = "fukasawah@dummy.example",
                        name = "fukasawah"
                    }
                }
            });

            JObject mailObject = JObject.Parse(data);

            Assert.Throws(typeof(JSchemaValidationException), () => mailObject.Validate(mailSchema));
        }
        [Fact]
        public void Test_Fromがないとエラー()
        {
            var gen = new JSchemaGenerator();
            gen.DefaultRequired = Required.Default;

            var mailSchema = gen.Generate(typeof(Mail));

            var data = JsonConvert.SerializeObject(new
            {
                to = new
                {
                    email = "rd@dummy.example",
                    name = "RD",
                }
            });

            JObject mailObject = JObject.Parse(data);

            Assert.Throws(typeof(JSchemaValidationException), () => mailObject.Validate(mailSchema));
        }
        [Fact]
        public void Test_Fromが1つ以上ないとエラー()
        {
            var gen = new JSchemaGenerator();
            gen.DefaultRequired = Required.Default;

            var mailSchema = gen.Generate(typeof(Mail));

            var data = JsonConvert.SerializeObject(new
            {
                to = new
                {
                    email = "rd@dummy.example",
                    name = "RD",
                },
                from = new object[0]
            });

            JObject mailObject = JObject.Parse(data);

            Assert.Throws(typeof(JSchemaValidationException), () => mailObject.Validate(mailSchema));
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
