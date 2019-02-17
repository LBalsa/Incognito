using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour, IInteractable
{
    public int arrayReference;

    public void InteractEyes()
    {
        Read();

        //for (int i = 0; i < Journal.inst.reportEntries.Count; i++)
        //{
        //    if (Journal.inst.reportEntries[i].unlocked)
        //    {
        //        Journal.inst.reportEntries[i].unlocked = true;
        //    }
        //}

        //int x = 0;
        //foreach (JournalEntry entry in Journal.inst.reportEntries)
        //{
        //    if (!entry.unlocked)
        //    {
        //        entry.Unlock(arrayReference, Journal.inst.reportDatabase);
        //        break;
        //    }
        //    x++;
        //}
    }

    public void InteractHands()
    {
        Read();
    }

    void Read()
    {
        JournalEntry je = Instantiate(Journal.inst.journalEntryPrefab, Journal.inst.reportList.transform).GetComponent<JournalEntry>();
        je.Unlock(arrayReference, Journal.inst.reportDatabase);

        GUINote.inst.note.text = Journal.inst.reportDatabase.entries[arrayReference].main;
        GUINote.inst.Toggle();
        Destroy(this.gameObject);
    }

}
