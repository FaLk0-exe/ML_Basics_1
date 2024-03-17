namespace ML_Basics_1.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Attribute = ML_Basics_1.Models.Attribute;

public class LearningSystem
{
    private const int MAX = 100;
    private List<Attribute> mayHaveDatabase = new List<Attribute>();
    private List<Attribute> mustHaveDatabase = new List<Attribute>();
    private StreamWriter protocolFile;

    public void Start()
    {
        Console.WriteLine("Enter a name for your task, please:");
        string fileName = Console.ReadLine() + ".ptc";

        try
        {
            protocolFile = new StreamWriter(fileName);
            Console.WriteLine($"File \"{fileName}\" is opened for protocol.");

            while (true)
            {
                Console.Write("(L)earn, (D)isplay, or (Q)uit? ");
                protocolFile.Write("(L)earn, (D)isplay, or (Q)uit? ");
                char choice = Char.ToLower(Console.ReadKey().KeyChar);
                protocolFile.WriteLine($" {choice}");

                switch (choice)
                {
                    case 'l':
                        Learn();
                        break;
                    case 'd':
                        Display();
                        break;
                    case 'q':
                        protocolFile.Close();
                        Console.WriteLine("\nThat is all!");
                        return;
                }

                Console.WriteLine();
                protocolFile.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private void Learn()
    {
        while (true)
        {
            Console.WriteLine("\nEnter an example.");
            protocolFile.WriteLine("\nEnter an example.");

            var example = GetExample();
            if (example == null) return;

            int index = FindMayHave(example.Subject, example.Verb, example.Object);
            if (index == -1)
            {
                AssertMayHave(example);
                Generalize(example);
            }

            Console.WriteLine("Enter a near-miss (NL to skip).");
            protocolFile.WriteLine("\nEnter a near-miss (NL to skip).");
            var nearMiss = GetExample();
            if (nearMiss != null)
            {
                Restrict(nearMiss);
            }
        }
    }

    private Attribute GetExample()
    {
        Console.Write("\nSubject: ");
        string subject = Console.ReadLine();
        if (string.IsNullOrEmpty(subject))
        {
            subject = "<NL>";
        }
        protocolFile.WriteLine($"\nSubject: {subject}");

        if (subject == "<NL>") return null;

        Console.Write("Verb: ");
        string verb = Console.ReadLine();

        Console.Write("Object: ");
        string obj = Console.ReadLine();

        protocolFile.WriteLine($"Verb: {verb}\nObject: {obj}");

        return new Attribute { Subject = subject, Verb = verb, Object = obj, Active = true };
    }

    private int FindMayHave(string subject, string verb, string obj)
    {
        for (int i = 0; i < mayHaveDatabase.Count; i++)
        {
            var attr = mayHaveDatabase[i];
            if (attr.Subject == subject && attr.Verb == verb && attr.Object == obj && attr.Active)
            {
                return i;
            }
        }
        return -1;
    }

    private void AssertMayHave(Attribute attr)
    {
        if (mayHaveDatabase.Count < MAX)
        {
            mayHaveDatabase.Add(attr);
        }
        else
        {
            Console.WriteLine("Out of memory for may-have database.");
        }
    }

    private void Generalize(Attribute attr)
    {
        for (int t = 0; t < mayHaveDatabase.Count; t++)
        {
            if (mayHaveDatabase[t].Subject != attr.Subject &&  
                mayHaveDatabase[t].Verb == attr.Verb &&
                mayHaveDatabase[t].Object == attr.Object && 
                mayHaveDatabase[t].Active)
            {
                mayHaveDatabase[t].Subject += " or " + attr.Subject;
            }
        }

        for (int t = 0; t < mayHaveDatabase.Count; t++)
        {
            if (mayHaveDatabase[t].Subject == attr.Subject &&
                mayHaveDatabase[t].Verb == attr.Verb &&
                mayHaveDatabase[t].Object != attr.Object && 
                mayHaveDatabase[t].Active)
            {
                mayHaveDatabase[t].Object += " or " + attr.Object;
            }
        }


        for (int t = 0; t < mustHaveDatabase.Count; t++)
        {
            if (mustHaveDatabase[t].Subject != attr.Subject && 
                mustHaveDatabase[t].Verb == attr.Verb &&
                mustHaveDatabase[t].Object == attr.Object)
            {
                mustHaveDatabase[t].Subject += " or " + attr.Subject;
                int i = FindMayHave(attr.Subject, attr.Verb, attr.Object);
                if (i != -1) mayHaveDatabase[i].Active = false;
            }
        }

        for (int t = 0; t < mustHaveDatabase.Count; t++)
        {
            if (mustHaveDatabase[t].Subject == attr.Subject &&
                mustHaveDatabase[t].Verb == attr.Verb &&
                mustHaveDatabase[t].Object != attr.Object)
            {
                mustHaveDatabase[t].Object += " or " + attr.Object;
                int i = FindMayHave(attr.Subject, attr.Verb, attr.Object);
                if (i != -1) mayHaveDatabase[i].Active = false; 
            }
        }
    }

    private void Restrict(Attribute attr)
    {

        string verb = attr.Verb.ToLower();
        if (!verb.StartsWith("not")) return;
        
        for (int i = 0; i < mayHaveDatabase.Count; i++)
        {
            var mayAttr = mayHaveDatabase[i];
            if (mayAttr.Verb == attr.Verb.Substring(4) &&
                mayAttr.Subject == attr.Subject &&
                mayAttr.Object == attr.Object &&
                mayAttr.Active)
            {
                AssertMustHave(mayAttr);
                mayAttr.Active = false;
                return;
            }
        }
    }

    private void AssertMustHave(Attribute attr)
    {
        if (mustHaveDatabase.Count < MAX)
        {
            mustHaveDatabase.Add(attr);
        }
        else
        {
            Console.WriteLine("Out of memory for must-have database.");
        }
    }

    private void Display()
    {
        Console.WriteLine("\nDisplay of description:\n\nMay have:");
        protocolFile.WriteLine("\nDisplay of description:\n\nMay have:");

        foreach (var attr in mayHaveDatabase.Where(a => a.Active))
        {
            Console.WriteLine($" {attr.Subject} {attr.Verb} {attr.Object}");
            protocolFile.WriteLine($" {attr.Subject} {attr.Verb} {attr.Object}");
        }

        Console.WriteLine("\nMust have:");
        protocolFile.WriteLine("\nMust have:");

        foreach (var attr in mustHaveDatabase)
        {
            Console.WriteLine($" {attr.Subject} {attr.Verb} {attr.Object}");
            protocolFile.WriteLine($" {attr.Subject} {attr.Verb} {attr.Object}");
        }
    }
}
