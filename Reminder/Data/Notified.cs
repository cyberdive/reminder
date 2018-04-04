using System.Collections.Generic;
using System.Linq;

namespace Reminder
{
    //liste non stockée qui mémorise si une tâche a été notifiée
    //afin de ne pas envoyer plusieurs fois la notification au PC

    public class NotifiedToday
    {
        private List<Tache> Liste { get; set; } = new List<Tache>();

        
        public bool IsNotified(Tache t)
        {
            var n = Liste.Where(i => i.ID == t.ID).FirstOrDefault();

            if (n == null)
            {
                Liste.Add(t);
                return false;
            }
            else
            {
                if (t.Equals(n))
                {
                    return true;
                }
                else
                {
                    RemoveTache(t.ID);
                    Liste.Add(t);
                    return false;
                }
            }
        }

        private void RemoveTache(long id)
        {
            //remove the task from the list here
            Liste = Liste.Where(note => note.ID != id).ToList();
        }


    }
    
}
