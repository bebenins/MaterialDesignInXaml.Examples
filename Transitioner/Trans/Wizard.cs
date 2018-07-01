using System.Collections.Generic;

namespace Trans
{
    public class Wizard
    {
        public class Question
        {
            public int ID { get; set; }
            public string Text { get; set; }
            public List<Choice> Choices { get; set; }
        }

        public class Choice
        {
            public bool IsSelected { get; set; }
            public string Text { get; set; }
            public int NextQuestionID { get; set; }
        }

        public int ID { get; set; }
        public string Description { get; set; }
        public List<Question> Questions { get; set; }
    }
}