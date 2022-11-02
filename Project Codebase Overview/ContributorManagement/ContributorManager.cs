﻿using Project_Codebase_Overview.ContributorManagement.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.ContributorManagement
{
    internal class ContributorManager
    {
        Dictionary<string, Author> Authors;
        Dictionary<string, Author> FileCreators; //Dict <filepath, creator> 
        PCOTeam SelectedTeam;
        
        private static ContributorManager Instance = null;

        public static void ResetInstance()
        { //TODO, make into a state and put in PCOState
            Instance = null;
            PCOColorPicker.ResetInstance();
        }

        private ContributorManager()
        {
            Authors = new Dictionary<string, Author>();
            FileCreators = new Dictionary<string, Author>();
        }

        public static ContributorManager GetInstance()
        {
            if(Instance == null)
            {
                Instance = new ContributorManager();
            }
            return Instance;
        }

        public void UpdateCreator(string path, string email)
        {
            FileCreators[path] = GetAuthor(email);
        }

        public Author GetCreatorFromPath(string path)
        {
            if (FileCreators.TryGetValue(path, out Author creator))
            {
                if (creator.OverAuthor != null)
                {
                    return creator.OverAuthor;
                }
                return creator;
            }
            return null;
        }

        private void AddAuthor(string email, string name)
        {
            Author author = new Author(email, name);
            var colorPicker = PCOColorPicker.GetInstance();
            author.Color = colorPicker.AssignAuthorColor();
            this.Authors.Add(email, author);
        }

        public Author GetAuthor (string email)
        {
            if (this.Authors.TryGetValue(email, out Author author))
            {
                if (author.OverAuthor != null)
                {
                    return author.OverAuthor;
                }
                return author;
            }
            return null;
        }

        public void InitializeAuthor(string email, string name)
        {
            if (this.Authors.ContainsKey(email))
            {
                this.Authors[email].AddAlias(name);
            } else
            {
                this.AddAuthor(email, name);
            }
        }

        public List<Author> GetAllAuthors()
        {
            return Authors.Values.Where(author => author.OverAuthor == null).ToList();
        }

        public void SetSelectedTeam(PCOTeam team)
        {
            SelectedTeam = team;
        }
        
        public PCOTeam GetSelectedTeam()
        {
            return SelectedTeam;
        }
    }
}
