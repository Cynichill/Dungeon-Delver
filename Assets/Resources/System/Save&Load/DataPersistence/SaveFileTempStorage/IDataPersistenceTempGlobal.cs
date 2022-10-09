using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDataPersistenceTempGlobal
{
    void LoadTempGlobal(TempGlobalData data);
    void SaveTempGlobal(TempGlobalData data);
}
