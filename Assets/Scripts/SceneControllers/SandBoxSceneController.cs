using System.Collections.Generic;
using UnityEngine;

public class SandBoxSceneController : MonoBehaviour {

    [SerializeField] private List<Cube> _cubes = new List<Cube>();
    
    public void ResetCubes() {
        foreach(Cube cube in _cubes) {
            cube.ResetPoseCube();
        }
    }
}
