using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDataPersistenceOptions
{
    void LoadOptions(OptionsData data);
    void SaveOptions(OptionsData data);
}
