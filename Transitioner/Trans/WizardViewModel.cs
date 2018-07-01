using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace Trans
{
    public class WizardViewModel : ViewModelBase
    {
        private Wizard ActiveWizard;
        private bool IsEnd;

        public ICommand CommandPreviousSlide { get; }
        public ICommand CommandNextSlide { get; }

        private string m_description;
        public string Description
        {
            get => m_description;
            set => Set(ref m_description, value);
        }

        private int m_slideIndex;
        public int SlideIndex
        {
            get => m_slideIndex;
            set => Set(ref m_slideIndex, value);
        }

        private ObservableCollection<Wizard.Question> m_dynamicQuestions;
        public ObservableCollection<Wizard.Question> DynamicQuestions
        {
            get => m_dynamicQuestions;
            set => Set(ref m_dynamicQuestions, value);
        }

        public WizardViewModel()
        {
            CommandPreviousSlide = new RelayCommand(OnPrevious, CanPrevious);
            CommandNextSlide = new RelayCommand(OnNext, CanNext);

            // Contruct wizard
            Wizard wizard = new Wizard();
            wizard.Description = "Testing wizard";
            wizard.ID = 100;
            wizard.Questions = new List<Wizard.Question>();
            for (int q = 0; q < 6; q++)
            {
                Wizard.Question question = new Wizard.Question();
                question.ID = q;
                question.Text = String.Format("Question ID {0}", q);

                question.Choices = new List<Wizard.Choice>();
                for (int c = 0; c < 6; c++)
                {
                    Wizard.Choice choice = new Wizard.Choice();
                    choice.Text = String.Format("Choice ID {0}", c);
                    choice.NextQuestionID = c;
                    question.Choices.Add(choice);
                }
                wizard.Questions.Add(question);
            }

            ActiveWizard = wizard;
            Description = wizard.Description;

            DynamicQuestions = new ObservableCollection<Wizard.Question>();
            // Get first quesiton
            DynamicQuestions.Add(GetQuestion(0));
            SlideIndex = 0;
        }

        private Wizard.Question GetQuestion(int id)
        {
            Wizard.Question question = ActiveWizard.Questions.FirstOrDefault(q => q.ID == id);
            if (question == null)
            {
                question = new Wizard.Question();
                question.Text = String.Format("Error!{1}Question ID {0} does not exist or previous Choice list was empty.{1}Update XML file to fix this issue.", id, Environment.NewLine);
                question.Choices = new List<Wizard.Choice>();
                Wizard.Choice choice = new Wizard.Choice();
                choice.Text = "OK";
                choice.NextQuestionID = -1;
                question.Choices.Add(choice);
            }

            if (question.Choices.Count != 0)
                question.Choices.ElementAt(0).IsSelected = true; // Pre-select first choice by default

            return question;
        }

        private bool CanPrevious()
        {
            int? previousIndex = GetPreviousSlideIndex();

            return previousIndex != null && previousIndex != SlideIndex;
        }

        private bool CanNext()
        {
            int? nextSlideIndex = GetNextSlideIndex();

            return nextSlideIndex != null && nextSlideIndex != SlideIndex;
        }

        private void OnPrevious()
        {
            IsEnd = false;
            int currentSlideIndex = SlideIndex;

            // Progress to this slide
            int? previousSlideIndex = GetPreviousSlideIndex();
            if (previousSlideIndex != null)
                SlideIndex = previousSlideIndex.Value;

            // Remove slide that backtracked from
            DynamicQuestions.RemoveAt(currentSlideIndex);
        }

        private void OnNext()
        {
            // Find next question ID
            int nextQuestionID = -1;
            if (DynamicQuestions[SlideIndex].Choices.Count != 0)
                nextQuestionID = DynamicQuestions[SlideIndex].Choices.First(c => c.IsSelected).NextQuestionID;

            // Get next question and add to slides
            Wizard.Question question = GetQuestion(nextQuestionID);
            if (!DynamicQuestions.Contains(question))
                DynamicQuestions.Add(question);

            // Progress to this slide
            int? nextSlideIndex = GetNextSlideIndex();
            if (nextSlideIndex != null)
                SlideIndex = nextSlideIndex.Value;

            // Check for dead end
            if (nextQuestionID == -1 || question.Choices.First().NextQuestionID == -1)
                IsEnd = true;
        }

        private int? GetNextSlideIndex()
        {
            return !IsEnd ? SlideIndex + 1 : SlideIndex;
        }

        private int? GetPreviousSlideIndex()
        {
            return SlideIndex == 0 ? SlideIndex : SlideIndex - 1;
        }
    }
}
