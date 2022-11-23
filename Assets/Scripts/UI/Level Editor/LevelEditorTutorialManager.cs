using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SourceConsole;
using System.Text;

public class LevelEditorTutorialManager : MonoBehaviour {
    private static LevelEditorTutorialManager Singleton;

    public static void StartTutorial()
    {
        if (Singleton == null) return;

        Singleton.StartCoroutine(Singleton.EnableTutorial());
    }

    [Header("UI stuff")]
    [SerializeField]
    private LerpCanvasGroup tutorialTextCanvasGroup;
    [SerializeField]
    private TMP_Text tutorialText;
    [SerializeField]
    private Button nextStageButton;
    [Header("Actual tutorial elements")]
    [SerializeField]
    private GameObject buildLevelOutline;
    [SerializeField]
    private GameObject launchHalo;
    [SerializeField]
    private GameObject theRealLandPad;
    [SerializeField]
    private GameObject landHalo;
    [SerializeField]
    private GameObject sensorHalo;
    [SerializeField]
    private GameObject doorHalo;

    [SerializeField]
    private RectTransform rectTransform;
    private Rect rect;

    private int lastTutorialStage = 0;
    private int tutorialStage = 0;
    private bool enableTutorial = false;

    [ConVar]
    public static int LevelEditorTutorial_Stage {
        get
        {
            if (Singleton == null) return -1;

            return Singleton.tutorialStage;
        }
        set
        {
            if (Singleton == null) return;

            Singleton.tutorialStage = value;
        }
    }

    private LevelEntity doorEntity;
    private LevelEntity sensorEntity;

    private bool shouldFollowWorktablePosition;
    private Vector2 followWorktablePosition;

    private Vector2[] tutorialEmptyPositions = new Vector2[] {
        new Vector2(115,72),
        new Vector2(116,72),
        new Vector2(117,72),
        new Vector2(118,72),
        new Vector2(119,72),
        new Vector2(120,72),
        new Vector2(121,72),
        new Vector2(124,72),
        new Vector2(125,72),
        new Vector2(126,72),
        new Vector2(127,72),
        new Vector2(128,72),
        new Vector2(129,72),
        new Vector2(130,72),
        new Vector2(131,72),
        new Vector2(132,72),
        new Vector2(133,72),
        new Vector2(134,72),
        new Vector2(135,72),
        new Vector2(108,73),
        new Vector2(109,73),
        new Vector2(110,73),
        new Vector2(111,73),
        new Vector2(112,73),
        new Vector2(113,73),
        new Vector2(114,73),
        new Vector2(115,73),
        new Vector2(116,73),
        new Vector2(117,73),
        new Vector2(118,73),
        new Vector2(119,73),
        new Vector2(120,73),
        new Vector2(121,73),
        new Vector2(122,73),
        new Vector2(123,73),
        new Vector2(124,73),
        new Vector2(125,73),
        new Vector2(126,73),
        new Vector2(127,73),
        new Vector2(128,73),
        new Vector2(129,73),
        new Vector2(130,73),
        new Vector2(131,73),
        new Vector2(132,73),
        new Vector2(133,73),
        new Vector2(134,73),
        new Vector2(135,73),
        new Vector2(136,73),
        new Vector2(137,73),
        new Vector2(105,74),
        new Vector2(106,74),
        new Vector2(107,74),
        new Vector2(108,74),
        new Vector2(109,74),
        new Vector2(110,74),
        new Vector2(111,74),
        new Vector2(112,74),
        new Vector2(113,74),
        new Vector2(114,74),
        new Vector2(115,74),
        new Vector2(116,74),
        new Vector2(117,74),
        new Vector2(118,74),
        new Vector2(119,74),
        new Vector2(120,74),
        new Vector2(121,74),
        new Vector2(122,74),
        new Vector2(123,74),
        new Vector2(124,74),
        new Vector2(125,74),
        new Vector2(126,74),
        new Vector2(127,74),
        new Vector2(128,74),
        new Vector2(129,74),
        new Vector2(130,74),
        new Vector2(131,74),
        new Vector2(132,74),
        new Vector2(133,74),
        new Vector2(134,74),
        new Vector2(135,74),
        new Vector2(136,74),
        new Vector2(137,74),
        new Vector2(138,74),
        new Vector2(98,75),
        new Vector2(99,75),
        new Vector2(100,75),
        new Vector2(101,75),
        new Vector2(102,75),
        new Vector2(103,75),
        new Vector2(104,75),
        new Vector2(105,75),
        new Vector2(106,75),
        new Vector2(107,75),
        new Vector2(108,75),
        new Vector2(109,75),
        new Vector2(110,75),
        new Vector2(111,75),
        new Vector2(112,75),
        new Vector2(113,75),
        new Vector2(114,75),
        new Vector2(115,75),
        new Vector2(116,75),
        new Vector2(117,75),
        new Vector2(118,75),
        new Vector2(119,75),
        new Vector2(120,75),
        new Vector2(121,75),
        new Vector2(122,75),
        new Vector2(123,75),
        new Vector2(124,75),
        new Vector2(125,75),
        new Vector2(126,75),
        new Vector2(127,75),
        new Vector2(128,75),
        new Vector2(129,75),
        new Vector2(130,75),
        new Vector2(131,75),
        new Vector2(132,75),
        new Vector2(133,75),
        new Vector2(134,75),
        new Vector2(135,75),
        new Vector2(136,75),
        new Vector2(137,75),
        new Vector2(138,75),
        new Vector2(139,75),
        new Vector2(64,76),
        new Vector2(65,76),
        new Vector2(66,76),
        new Vector2(67,76),
        new Vector2(68,76),
        new Vector2(69,76),
        new Vector2(70,76),
        new Vector2(71,76),
        new Vector2(72,76),
        new Vector2(73,76),
        new Vector2(74,76),
        new Vector2(75,76),
        new Vector2(76,76),
        new Vector2(77,76),
        new Vector2(78,76),
        new Vector2(79,76),
        new Vector2(80,76),
        new Vector2(81,76),
        new Vector2(82,76),
        new Vector2(83,76),
        new Vector2(84,76),
        new Vector2(85,76),
        new Vector2(86,76),
        new Vector2(87,76),
        new Vector2(88,76),
        new Vector2(89,76),
        new Vector2(90,76),
        new Vector2(91,76),
        new Vector2(92,76),
        new Vector2(93,76),
        new Vector2(94,76),
        new Vector2(95,76),
        new Vector2(96,76),
        new Vector2(97,76),
        new Vector2(98,76),
        new Vector2(99,76),
        new Vector2(100,76),
        new Vector2(101,76),
        new Vector2(102,76),
        new Vector2(103,76),
        new Vector2(104,76),
        new Vector2(105,76),
        new Vector2(106,76),
        new Vector2(107,76),
        new Vector2(108,76),
        new Vector2(109,76),
        new Vector2(110,76),
        new Vector2(111,76),
        new Vector2(112,76),
        new Vector2(113,76),
        new Vector2(114,76),
        new Vector2(115,76),
        new Vector2(116,76),
        new Vector2(117,76),
        new Vector2(118,76),
        new Vector2(119,76),
        new Vector2(120,76),
        new Vector2(121,76),
        new Vector2(122,76),
        new Vector2(123,76),
        new Vector2(124,76),
        new Vector2(125,76),
        new Vector2(126,76),
        new Vector2(127,76),
        new Vector2(128,76),
        new Vector2(129,76),
        new Vector2(130,76),
        new Vector2(131,76),
        new Vector2(132,76),
        new Vector2(133,76),
        new Vector2(134,76),
        new Vector2(135,76),
        new Vector2(136,76),
        new Vector2(137,76),
        new Vector2(138,76),
        new Vector2(139,76),
        new Vector2(140,76),
        new Vector2(64,77),
        new Vector2(65,77),
        new Vector2(66,77),
        new Vector2(67,77),
        new Vector2(68,77),
        new Vector2(69,77),
        new Vector2(70,77),
        new Vector2(71,77),
        new Vector2(72,77),
        new Vector2(73,77),
        new Vector2(74,77),
        new Vector2(75,77),
        new Vector2(76,77),
        new Vector2(77,77),
        new Vector2(78,77),
        new Vector2(79,77),
        new Vector2(80,77),
        new Vector2(81,77),
        new Vector2(82,77),
        new Vector2(83,77),
        new Vector2(84,77),
        new Vector2(85,77),
        new Vector2(86,77),
        new Vector2(87,77),
        new Vector2(88,77),
        new Vector2(89,77),
        new Vector2(90,77),
        new Vector2(91,77),
        new Vector2(92,77),
        new Vector2(93,77),
        new Vector2(94,77),
        new Vector2(95,77),
        new Vector2(96,77),
        new Vector2(97,77),
        new Vector2(98,77),
        new Vector2(99,77),
        new Vector2(100,77),
        new Vector2(101,77),
        new Vector2(102,77),
        new Vector2(103,77),
        new Vector2(104,77),
        new Vector2(105,77),
        new Vector2(106,77),
        new Vector2(107,77),
        new Vector2(108,77),
        new Vector2(109,77),
        new Vector2(110,77),
        new Vector2(111,77),
        new Vector2(112,77),
        new Vector2(113,77),
        new Vector2(114,77),
        new Vector2(115,77),
        new Vector2(116,77),
        new Vector2(117,77),
        new Vector2(118,77),
        new Vector2(119,77),
        new Vector2(120,77),
        new Vector2(121,77),
        new Vector2(122,77),
        new Vector2(123,77),
        new Vector2(124,77),
        new Vector2(125,77),
        new Vector2(126,77),
        new Vector2(127,77),
        new Vector2(128,77),
        new Vector2(129,77),
        new Vector2(130,77),
        new Vector2(131,77),
        new Vector2(132,77),
        new Vector2(133,77),
        new Vector2(134,77),
        new Vector2(135,77),
        new Vector2(136,77),
        new Vector2(137,77),
        new Vector2(138,77),
        new Vector2(139,77),
        new Vector2(140,77),
        new Vector2(64,78),
        new Vector2(65,78),
        new Vector2(66,78),
        new Vector2(67,78),
        new Vector2(68,78),
        new Vector2(69,78),
        new Vector2(70,78),
        new Vector2(71,78),
        new Vector2(72,78),
        new Vector2(73,78),
        new Vector2(74,78),
        new Vector2(75,78),
        new Vector2(76,78),
        new Vector2(77,78),
        new Vector2(78,78),
        new Vector2(79,78),
        new Vector2(80,78),
        new Vector2(81,78),
        new Vector2(82,78),
        new Vector2(83,78),
        new Vector2(84,78),
        new Vector2(85,78),
        new Vector2(86,78),
        new Vector2(87,78),
        new Vector2(88,78),
        new Vector2(89,78),
        new Vector2(90,78),
        new Vector2(91,78),
        new Vector2(92,78),
        new Vector2(93,78),
        new Vector2(94,78),
        new Vector2(95,78),
        new Vector2(96,78),
        new Vector2(97,78),
        new Vector2(98,78),
        new Vector2(99,78),
        new Vector2(100,78),
        new Vector2(101,78),
        new Vector2(102,78),
        new Vector2(103,78),
        new Vector2(104,78),
        new Vector2(105,78),
        new Vector2(106,78),
        new Vector2(107,78),
        new Vector2(108,78),
        new Vector2(109,78),
        new Vector2(110,78),
        new Vector2(111,78),
        new Vector2(112,78),
        new Vector2(113,78),
        new Vector2(114,78),
        new Vector2(115,78),
        new Vector2(116,78),
        new Vector2(117,78),
        new Vector2(118,78),
        new Vector2(119,78),
        new Vector2(120,78),
        new Vector2(121,78),
        new Vector2(122,78),
        new Vector2(123,78),
        new Vector2(124,78),
        new Vector2(125,78),
        new Vector2(126,78),
        new Vector2(127,78),
        new Vector2(128,78),
        new Vector2(129,78),
        new Vector2(130,78),
        new Vector2(131,78),
        new Vector2(132,78),
        new Vector2(133,78),
        new Vector2(134,78),
        new Vector2(135,78),
        new Vector2(136,78),
        new Vector2(137,78),
        new Vector2(138,78),
        new Vector2(139,78),
        new Vector2(140,78),
        new Vector2(64,79),
        new Vector2(65,79),
        new Vector2(66,79),
        new Vector2(67,79),
        new Vector2(68,79),
        new Vector2(69,79),
        new Vector2(70,79),
        new Vector2(71,79),
        new Vector2(72,79),
        new Vector2(73,79),
        new Vector2(74,79),
        new Vector2(75,79),
        new Vector2(76,79),
        new Vector2(77,79),
        new Vector2(78,79),
        new Vector2(79,79),
        new Vector2(80,79),
        new Vector2(81,79),
        new Vector2(82,79),
        new Vector2(83,79),
        new Vector2(84,79),
        new Vector2(85,79),
        new Vector2(86,79),
        new Vector2(87,79),
        new Vector2(88,79),
        new Vector2(89,79),
        new Vector2(90,79),
        new Vector2(91,79),
        new Vector2(92,79),
        new Vector2(93,79),
        new Vector2(94,79),
        new Vector2(95,79),
        new Vector2(96,79),
        new Vector2(97,79),
        new Vector2(98,79),
        new Vector2(99,79),
        new Vector2(100,79),
        new Vector2(101,79),
        new Vector2(102,79),
        new Vector2(103,79),
        new Vector2(104,79),
        new Vector2(105,79),
        new Vector2(106,79),
        new Vector2(107,79),
        new Vector2(108,79),
        new Vector2(109,79),
        new Vector2(110,79),
        new Vector2(111,79),
        new Vector2(112,79),
        new Vector2(113,79),
        new Vector2(114,79),
        new Vector2(115,79),
        new Vector2(116,79),
        new Vector2(117,79),
        new Vector2(118,79),
        new Vector2(119,79),
        new Vector2(120,79),
        new Vector2(121,79),
        new Vector2(122,79),
        new Vector2(123,79),
        new Vector2(124,79),
        new Vector2(125,79),
        new Vector2(126,79),
        new Vector2(127,79),
        new Vector2(128,79),
        new Vector2(129,79),
        new Vector2(130,79),
        new Vector2(131,79),
        new Vector2(132,79),
        new Vector2(133,79),
        new Vector2(134,79),
        new Vector2(135,79),
        new Vector2(136,79),
        new Vector2(137,79),
        new Vector2(138,79),
        new Vector2(139,79),
        new Vector2(140,79),
        new Vector2(141,79),
        new Vector2(64,80),
        new Vector2(65,80),
        new Vector2(66,80),
        new Vector2(67,80),
        new Vector2(68,80),
        new Vector2(69,80),
        new Vector2(70,80),
        new Vector2(71,80),
        new Vector2(72,80),
        new Vector2(73,80),
        new Vector2(74,80),
        new Vector2(75,80),
        new Vector2(76,80),
        new Vector2(77,80),
        new Vector2(78,80),
        new Vector2(79,80),
        new Vector2(80,80),
        new Vector2(81,80),
        new Vector2(82,80),
        new Vector2(83,80),
        new Vector2(84,80),
        new Vector2(85,80),
        new Vector2(86,80),
        new Vector2(87,80),
        new Vector2(88,80),
        new Vector2(89,80),
        new Vector2(90,80),
        new Vector2(91,80),
        new Vector2(92,80),
        new Vector2(93,80),
        new Vector2(94,80),
        new Vector2(95,80),
        new Vector2(96,80),
        new Vector2(97,80),
        new Vector2(98,80),
        new Vector2(99,80),
        new Vector2(100,80),
        new Vector2(101,80),
        new Vector2(102,80),
        new Vector2(103,80),
        new Vector2(104,80),
        new Vector2(105,80),
        new Vector2(106,80),
        new Vector2(107,80),
        new Vector2(108,80),
        new Vector2(109,80),
        new Vector2(110,80),
        new Vector2(111,80),
        new Vector2(112,80),
        new Vector2(113,80),
        new Vector2(114,80),
        new Vector2(115,80),
        new Vector2(116,80),
        new Vector2(117,80),
        new Vector2(118,80),
        new Vector2(119,80),
        new Vector2(120,80),
        new Vector2(121,80),
        new Vector2(122,80),
        new Vector2(123,80),
        new Vector2(124,80),
        new Vector2(125,80),
        new Vector2(126,80),
        new Vector2(127,80),
        new Vector2(128,80),
        new Vector2(129,80),
        new Vector2(130,80),
        new Vector2(131,80),
        new Vector2(132,80),
        new Vector2(133,80),
        new Vector2(134,80),
        new Vector2(135,80),
        new Vector2(136,80),
        new Vector2(137,80),
        new Vector2(138,80),
        new Vector2(139,80),
        new Vector2(140,80),
        new Vector2(141,80),
        new Vector2(64,81),
        new Vector2(65,81),
        new Vector2(66,81),
        new Vector2(67,81),
        new Vector2(68,81),
        new Vector2(69,81),
        new Vector2(70,81),
        new Vector2(71,81),
        new Vector2(72,81),
        new Vector2(73,81),
        new Vector2(74,81),
        new Vector2(75,81),
        new Vector2(76,81),
        new Vector2(77,81),
        new Vector2(78,81),
        new Vector2(79,81),
        new Vector2(80,81),
        new Vector2(81,81),
        new Vector2(82,81),
        new Vector2(83,81),
        new Vector2(84,81),
        new Vector2(85,81),
        new Vector2(86,81),
        new Vector2(87,81),
        new Vector2(88,81),
        new Vector2(89,81),
        new Vector2(90,81),
        new Vector2(91,81),
        new Vector2(92,81),
        new Vector2(93,81),
        new Vector2(94,81),
        new Vector2(95,81),
        new Vector2(96,81),
        new Vector2(97,81),
        new Vector2(98,81),
        new Vector2(99,81),
        new Vector2(100,81),
        new Vector2(101,81),
        new Vector2(102,81),
        new Vector2(103,81),
        new Vector2(104,81),
        new Vector2(105,81),
        new Vector2(106,81),
        new Vector2(107,81),
        new Vector2(108,81),
        new Vector2(109,81),
        new Vector2(110,81),
        new Vector2(111,81),
        new Vector2(112,81),
        new Vector2(113,81),
        new Vector2(114,81),
        new Vector2(115,81),
        new Vector2(116,81),
        new Vector2(117,81),
        new Vector2(118,81),
        new Vector2(119,81),
        new Vector2(120,81),
        new Vector2(121,81),
        new Vector2(122,81),
        new Vector2(123,81),
        new Vector2(124,81),
        new Vector2(125,81),
        new Vector2(126,81),
        new Vector2(127,81),
        new Vector2(128,81),
        new Vector2(129,81),
        new Vector2(130,81),
        new Vector2(131,81),
        new Vector2(132,81),
        new Vector2(133,81),
        new Vector2(134,81),
        new Vector2(135,81),
        new Vector2(136,81),
        new Vector2(137,81),
        new Vector2(64,82),
        new Vector2(65,82),
        new Vector2(66,82),
        new Vector2(67,82),
        new Vector2(68,82),
        new Vector2(69,82),
        new Vector2(70,82),
        new Vector2(71,82),
        new Vector2(72,82),
        new Vector2(73,82),
        new Vector2(74,82),
        new Vector2(75,82),
        new Vector2(76,82),
        new Vector2(77,82),
        new Vector2(78,82),
        new Vector2(79,82),
        new Vector2(80,82),
        new Vector2(81,82),
        new Vector2(82,82),
        new Vector2(83,82),
        new Vector2(84,82),
        new Vector2(85,82),
        new Vector2(86,82),
        new Vector2(87,82),
        new Vector2(88,82),
        new Vector2(89,82),
        new Vector2(90,82),
        new Vector2(91,82),
        new Vector2(92,82),
        new Vector2(93,82),
        new Vector2(94,82),
        new Vector2(95,82),
        new Vector2(96,82),
        new Vector2(97,82),
        new Vector2(98,82),
        new Vector2(99,82),
        new Vector2(100,82),
        new Vector2(101,82),
        new Vector2(102,82),
        new Vector2(103,82),
        new Vector2(104,82),
        new Vector2(105,82),
        new Vector2(106,82),
        new Vector2(107,82),
        new Vector2(108,82),
        new Vector2(109,82),
        new Vector2(110,82),
        new Vector2(111,82),
        new Vector2(112,82),
        new Vector2(113,82),
        new Vector2(114,82),
        new Vector2(115,82),
        new Vector2(116,82),
        new Vector2(117,82),
        new Vector2(118,82),
        new Vector2(119,82),
        new Vector2(120,82),
        new Vector2(121,82),
        new Vector2(122,82),
        new Vector2(123,82),
        new Vector2(131,82),
        new Vector2(132,82),
        new Vector2(133,82),
        new Vector2(64,83),
        new Vector2(65,83),
        new Vector2(66,83),
        new Vector2(67,83),
        new Vector2(68,83),
        new Vector2(69,83),
        new Vector2(70,83),
        new Vector2(71,83),
        new Vector2(72,83),
        new Vector2(73,83),
        new Vector2(74,83),
        new Vector2(75,83),
        new Vector2(76,83),
        new Vector2(77,83),
        new Vector2(78,83),
        new Vector2(79,83),
        new Vector2(80,83),
        new Vector2(81,83),
        new Vector2(82,83),
        new Vector2(83,83),
        new Vector2(84,83),
        new Vector2(85,83),
        new Vector2(86,83),
        new Vector2(87,83),
        new Vector2(88,83),
        new Vector2(89,83),
        new Vector2(90,83),
        new Vector2(91,83),
        new Vector2(92,83),
        new Vector2(93,83),
        new Vector2(94,83),
        new Vector2(95,83),
        new Vector2(96,83),
        new Vector2(97,83),
        new Vector2(98,83),
        new Vector2(99,83),
        new Vector2(100,83),
        new Vector2(101,83),
        new Vector2(102,83),
        new Vector2(103,83),
        new Vector2(104,83),
        new Vector2(105,83),
        new Vector2(106,83),
        new Vector2(107,83),
        new Vector2(108,83),
        new Vector2(109,83),
        new Vector2(110,83),
        new Vector2(111,83),
        new Vector2(112,83),
        new Vector2(113,83),
        new Vector2(114,83),
        new Vector2(115,83),
        new Vector2(116,83),
        new Vector2(117,83),
        new Vector2(118,83),
        new Vector2(119,83),
        new Vector2(120,83),
        new Vector2(121,83),
        new Vector2(122,83),
        new Vector2(64,84),
        new Vector2(65,84),
        new Vector2(66,84),
        new Vector2(67,84),
        new Vector2(68,84),
        new Vector2(69,84),
        new Vector2(70,84),
        new Vector2(71,84),
        new Vector2(72,84),
        new Vector2(73,84),
        new Vector2(74,84),
        new Vector2(75,84),
        new Vector2(76,84),
        new Vector2(77,84),
        new Vector2(78,84),
        new Vector2(79,84),
        new Vector2(80,84),
        new Vector2(81,84),
        new Vector2(82,84),
        new Vector2(83,84),
        new Vector2(84,84),
        new Vector2(85,84),
        new Vector2(86,84),
        new Vector2(87,84),
        new Vector2(88,84),
        new Vector2(89,84),
        new Vector2(90,84),
        new Vector2(91,84),
        new Vector2(92,84),
        new Vector2(93,84),
        new Vector2(94,84),
        new Vector2(95,84),
        new Vector2(96,84),
        new Vector2(97,84),
        new Vector2(98,84),
        new Vector2(99,84),
        new Vector2(100,84),
        new Vector2(101,84),
        new Vector2(102,84),
        new Vector2(103,84),
        new Vector2(104,84),
        new Vector2(105,84),
        new Vector2(106,84),
        new Vector2(107,84),
        new Vector2(108,84),
        new Vector2(109,84),
        new Vector2(110,84),
        new Vector2(111,84),
        new Vector2(112,84),
        new Vector2(113,84),
        new Vector2(114,84),
        new Vector2(115,84),
        new Vector2(116,84),
        new Vector2(117,84),
        new Vector2(118,84),
        new Vector2(119,84),
        new Vector2(120,84),
        new Vector2(121,84),
        new Vector2(64,85),
        new Vector2(65,85),
        new Vector2(66,85),
        new Vector2(67,85),
        new Vector2(68,85),
        new Vector2(69,85),
        new Vector2(70,85),
        new Vector2(71,85),
        new Vector2(72,85),
        new Vector2(73,85),
        new Vector2(74,85),
        new Vector2(75,85),
        new Vector2(76,85),
        new Vector2(77,85),
        new Vector2(78,85),
        new Vector2(79,85),
        new Vector2(80,85),
        new Vector2(81,85),
        new Vector2(82,85),
        new Vector2(83,85),
        new Vector2(84,85),
        new Vector2(85,85),
        new Vector2(86,85),
        new Vector2(87,85),
        new Vector2(88,85),
        new Vector2(89,85),
        new Vector2(90,85),
        new Vector2(91,85),
        new Vector2(92,85),
        new Vector2(93,85),
        new Vector2(94,85),
        new Vector2(95,85),
        new Vector2(96,85),
        new Vector2(97,85),
        new Vector2(98,85),
        new Vector2(99,85),
        new Vector2(100,85),
        new Vector2(101,85),
        new Vector2(102,85),
        new Vector2(103,85),
        new Vector2(104,85),
        new Vector2(105,85),
        new Vector2(106,85),
        new Vector2(107,85),
        new Vector2(108,85),
        new Vector2(109,85),
        new Vector2(110,85),
        new Vector2(111,85),
        new Vector2(112,85),
        new Vector2(113,85),
        new Vector2(114,85),
        new Vector2(115,85),
        new Vector2(116,85),
        new Vector2(117,85),
        new Vector2(118,85),
        new Vector2(119,85),
        new Vector2(120,85),
        new Vector2(64,86),
        new Vector2(65,86),
        new Vector2(66,86),
        new Vector2(67,86),
        new Vector2(68,86),
        new Vector2(69,86),
        new Vector2(70,86),
        new Vector2(71,86),
        new Vector2(72,86),
        new Vector2(73,86),
        new Vector2(74,86),
        new Vector2(75,86),
        new Vector2(76,86),
        new Vector2(77,86),
        new Vector2(78,86),
        new Vector2(79,86),
        new Vector2(80,86),
        new Vector2(81,86),
        new Vector2(82,86),
        new Vector2(83,86),
        new Vector2(84,86),
        new Vector2(85,86),
        new Vector2(86,86),
        new Vector2(87,86),
        new Vector2(88,86),
        new Vector2(89,86),
        new Vector2(90,86),
        new Vector2(91,86),
        new Vector2(92,86),
        new Vector2(93,86),
        new Vector2(94,86),
        new Vector2(95,86),
        new Vector2(96,86),
        new Vector2(97,86),
        new Vector2(98,86),
        new Vector2(99,86),
        new Vector2(100,86),
        new Vector2(101,86),
        new Vector2(102,86),
        new Vector2(103,86),
        new Vector2(104,86),
        new Vector2(105,86),
        new Vector2(106,86),
        new Vector2(107,86),
        new Vector2(108,86),
        new Vector2(109,86),
        new Vector2(110,86),
        new Vector2(111,86),
        new Vector2(112,86),
        new Vector2(113,86),
        new Vector2(114,86),
        new Vector2(115,86),
        new Vector2(116,86),
        new Vector2(117,86),
        new Vector2(118,86),
        new Vector2(119,86),
        new Vector2(64,87),
        new Vector2(65,87),
        new Vector2(66,87),
        new Vector2(67,87),
        new Vector2(68,87),
        new Vector2(69,87),
        new Vector2(70,87),
        new Vector2(71,87),
        new Vector2(72,87),
        new Vector2(73,87),
        new Vector2(74,87),
        new Vector2(75,87),
        new Vector2(76,87),
        new Vector2(77,87),
        new Vector2(78,87),
        new Vector2(79,87),
        new Vector2(80,87),
        new Vector2(81,87),
        new Vector2(82,87),
        new Vector2(83,87),
        new Vector2(84,87),
        new Vector2(85,87),
        new Vector2(86,87),
        new Vector2(87,87),
        new Vector2(88,87),
        new Vector2(89,87),
        new Vector2(90,87),
        new Vector2(91,87),
        new Vector2(92,87),
        new Vector2(93,87),
        new Vector2(94,87),
        new Vector2(95,87),
        new Vector2(96,87),
        new Vector2(97,87),
        new Vector2(98,87),
        new Vector2(99,87),
        new Vector2(100,87),
        new Vector2(101,87),
        new Vector2(102,87),
        new Vector2(103,87),
        new Vector2(104,87),
        new Vector2(105,87),
        new Vector2(106,87),
        new Vector2(107,87),
        new Vector2(108,87),
        new Vector2(109,87),
        new Vector2(110,87),
        new Vector2(111,87),
        new Vector2(112,87),
        new Vector2(113,87),
        new Vector2(114,87),
        new Vector2(115,87),
        new Vector2(116,87),
        new Vector2(117,87),
        new Vector2(118,87),
        new Vector2(119,87),
        new Vector2(64,88),
        new Vector2(65,88),
        new Vector2(66,88),
        new Vector2(67,88),
        new Vector2(68,88),
        new Vector2(69,88),
        new Vector2(70,88),
        new Vector2(71,88),
        new Vector2(72,88),
        new Vector2(73,88),
        new Vector2(74,88),
        new Vector2(75,88),
        new Vector2(76,88),
        new Vector2(77,88),
        new Vector2(78,88),
        new Vector2(79,88),
        new Vector2(80,88),
        new Vector2(81,88),
        new Vector2(82,88),
        new Vector2(83,88),
        new Vector2(84,88),
        new Vector2(85,88),
        new Vector2(86,88),
        new Vector2(87,88),
        new Vector2(88,88),
        new Vector2(89,88),
        new Vector2(90,88),
        new Vector2(91,88),
        new Vector2(92,88),
        new Vector2(93,88),
        new Vector2(94,88),
        new Vector2(95,88),
        new Vector2(96,88),
        new Vector2(97,88),
        new Vector2(98,88),
        new Vector2(99,88),
        new Vector2(100,88),
        new Vector2(101,88),
        new Vector2(102,88),
        new Vector2(103,88),
        new Vector2(104,88),
        new Vector2(105,88),
        new Vector2(106,88),
        new Vector2(107,88),
        new Vector2(108,88),
        new Vector2(109,88),
        new Vector2(110,88),
        new Vector2(111,88),
        new Vector2(112,88),
        new Vector2(113,88),
        new Vector2(114,88),
        new Vector2(115,88),
        new Vector2(116,88),
        new Vector2(117,88),
        new Vector2(118,88),
        new Vector2(102,89),
        new Vector2(103,89),
        new Vector2(104,89),
        new Vector2(105,89),
        new Vector2(106,89),
        new Vector2(107,89),
        new Vector2(108,89),
        new Vector2(109,89),
        new Vector2(110,89),
        new Vector2(111,89),
        new Vector2(112,89),
        new Vector2(113,89),
        new Vector2(114,89),
        new Vector2(107,90)
    };

    [ConVar]
    public static bool LevelEditorTutorial_ShouldFollowPosition
    {
        get
        {
            if (Singleton == null) return false;

            return Singleton.shouldFollowWorktablePosition;
        }
        set
        {
            if (Singleton == null) return;

            Singleton.shouldFollowWorktablePosition = value;
        }
    }

    [ConCommand]
    public static void LevelEditorTutorial_FollowPosition(float x, float y)
    {
        if (Singleton == null) return;
        Singleton.followWorktablePosition = new Vector2(x, y);
    }

    [ConCommand]
    public static void LevelEditorTutorial_PrintEmptyLocations()
    {
        var output = new StringBuilder();

        var values = MarchingSquaresManager.GetValues();
        for (int y = 0; y < 150; y++)
        {
            for(int x = 0; x < 150; x++)
            {
                var value = values[x + y * MarchingSquaresManager.DataWidth];
                if(value < 0.5)
                {
                    output.Append($"({x},{y});");
                }
            }
        }

        Debug.Log(output.ToString());
        SourceConsole.SourceConsole.print(output.ToString());
    }

#if UNITY_EDITOR
    [ConCommand]
    public static void LevelEditorTutorial_ShowTutorialEmptyLocations()
    {
        if (Singleton == null) return;

        Debug.Log(Singleton.tutorialEmptyPositions.Length);

        var offset = new Vector2(18, 18);
        foreach(var point in Singleton.tutorialEmptyPositions)
        {
            var fixedPoint = point / 150f * 36 - offset;

            Debug.DrawLine(fixedPoint, fixedPoint + new Vector2(0, 0.5f), Color.red, 15);
        }
    }
#endif

    private LevelData levelData;

    private void Awake()
    {
        Singleton = this;

        tutorialTextCanvasGroup.ForceAlpha(0);

        buildLevelOutline.SetActive(false);
        launchHalo.SetActive(false);
        landHalo.SetActive(false);
        sensorHalo.SetActive(false);
        doorHalo.SetActive(false);
        
        nextStageButton.interactable = false;

        rect = rectTransform.rect;
    }

    private void Start()
    {
        levelData = LevelEditorManager.GetLevelData();
    }

    private IEnumerator EnableTutorial()
    {
        yield return new WaitForSeconds(0.45f);

        UpdateTutorialText();
        enableTutorial = true;

        StartCoroutine(CheckNextStageConditionsLoop());
    }

    private IEnumerator CheckNextStageConditionsLoop()
    {
        while (true)
        {
            CheckNextStageConditions();

            if (SceneManager.GetActiveScene().name != "Level Editor") break;

            yield return new WaitForSeconds(0.2f);
        }
    }

    private void OnDestroy()
    {
        Singleton = null;
    }

    public static void TriedToDeletePads()
    {
        if (Singleton == null) return;

        if(Singleton.tutorialStage == 2)
        {
            ShowTutorialText("Well, anything you want to delete EXCEPT the land or launch pads... We still need those! (Click to continue)");
            SteamCustomUtils.SetLevelEditorAchievement("DONT_DELETE_THAT");
        }
    }

    public static void ShowTutorialText(string text)
    {
        if (Singleton == null) return;

        Singleton.tutorialTextCanvasGroup.target = 1;
        Singleton.tutorialText.text = text;
    }

    public static void HideTutorialText()
    {
        if (Singleton == null) return;

        Singleton.tutorialTextCanvasGroup.target = 0;
    }

    private void UpdateTutorialText()
    {
        switch (tutorialStage)
        {
            case 0:
                ShowTutorialText("Hello! Let's begin by building a simple level outline. Hold down the left mouse button and drag the mouse over the white arrow outline. It doesn't have to be exact, but make sure that every tile has atleast one non-diagonal tile neighbor (meaning no diagonal corners)");
                buildLevelOutline.SetActive(true);
                break;
            case 1:
                ShowTutorialText("Excellent! Now let's move the landing pad to it's correct place, highlighted by the red fading icon to the right.");
                buildLevelOutline.SetActive(false);
                landHalo.SetActive(true);

                shouldFollowWorktablePosition = true;
                break;
            case 2:
                shouldFollowWorktablePosition = false;
                landHalo.SetActive(false);
                nextStageButton.interactable = true;
                CursorController.RemoveUser("HoverPointer");
                ShowTutorialText("Mistakes happen, and sometimes we place down tiles or entities we don't want. Luckily we have the eraser tool! Press 'A' on your keyboard to toggle between the eraser tool and hold down the left mouse button over anything you want to erase. (Click to continue)");
                break;
            case 3:
                shouldFollowWorktablePosition = true;
                followWorktablePosition = doorHalo.transform.position + new Vector3(0, -3);
                doorHalo.SetActive(true);
                ShowTutorialText("Lets place down a door in the middle of the level, in-between the launch and landing pad. Click the icon-button titled 'door' from the left panel and place the door over the highlighted area.");
                break;
            case 4:
                doorHalo.SetActive(false);
                followWorktablePosition = sensorHalo.transform.position + new Vector3(0, -1);
                ShowTutorialText("Great! Now we need a way to open the door. Place down a 'ship sensor pad' at the highlighted location (or anywhere else thats <i>inside</i> the level, keep in mind the player will need a way to get to the sensor).");
                sensorHalo.SetActive(true);
                break;
            case 5:
                sensorHalo.SetActive(false);
                followWorktablePosition = Vector2.Lerp(sensorHalo.transform.position, doorHalo.transform.position, 0.5f);
                followWorktablePosition.y = -1;
                ShowTutorialText("Now, if only the door and ship sensor were connected somehow! Hold down the left-alt key and click on the ship sensor, then on the door, and let go of the alt key to logic-connect the door and the ship sensor.");
                break;
            case 6:
                nextStageButton.interactable = true;
                shouldFollowWorktablePosition = false;
                ShowTutorialText("All done! If the level is enough to your liking, save your level by pressing 'CTRL+S', press 'escape' and click the 'playtest' button at the top of the screen!");
                break;
            case 14:
                ShowTutorialText("That's it... There is nothing else here...");
                break;
            default:
                nextStageButton.interactable = true;
                tutorialStage++;
                break;
        }
    }

    //TODO: Move this to regular level editor scene!

    private void GetIfPadsInsideLevel(out bool launchPadsInsideLevel, out bool landingPadsInsideLevel)
    {
        int numberOfLaunchingPads = 0;
        int numberOfLandingPads = 0;
        launchPadsInsideLevel = true;
        landingPadsInsideLevel = true;
        foreach (LevelObject obj in levelData.levelData)
        {
            if (obj.GetType() == typeof(LevelEntity))
            {
                LevelEntity ent = (LevelEntity)obj;
                if (ent.resourceName == "Ship Pads/Launch Pad")
                {
                    numberOfLaunchingPads++;
                    if (!levelData.IsPointInLevel(ent.GetPosition()))
                    {
                        launchPadsInsideLevel = false;
                    }
                }
                if (ent.resourceName == "Ship Pads/Land Pad")
                {
                    numberOfLandingPads++;
                    if (!levelData.IsPointInLevel(ent.GetPosition()))
                    {
                        landingPadsInsideLevel = false;
                    }
                }
            }
        }
    }

    private void CheckNextStageConditions()
    {
        levelData.SortTilesList();
        levelData.SortLines();

        switch (tutorialStage)
        {
            case 0:
                bool launchPadsInsideLevel;
                bool landingPadsInsideLevel;
                GetIfPadsInsideLevel(out launchPadsInsideLevel, out landingPadsInsideLevel);

                if (levelData.IsLevelInclosed() && levelData.GetTiles().Count > 15 && launchPadsInsideLevel && landingPadsInsideLevel) tutorialStage++;
                break;
            case 1:
                if (Vector2.Distance(levelData.GetLandPad().GetPosition(), new Vector2(8.8f, 0.7f)) < 0.1 && !LevelEditorCursor.IsCurrentlyMovingObject()) tutorialStage++;
                break;
            case 3:
                var doorObj = levelData.GetObjectAtPosition(new Vector2(4.5f, 2.5f)); //look for the door
                if(doorObj is LevelEntity && !LevelEditorCursor.IsCurrentlyMovingObject())
                {
                    var entity = (LevelEntity)doorObj;
                    if(entity.resourceName == "Door/Door")
                    {
                        doorEntity = entity;
                        tutorialStage++;
                    }
                }
                break;
            case 4:
                var sensorObj = levelData.GetEntityAtArea(new Vector2(2.5f, 0.6f), 0.5f); //look for the landing pad sensor
                if (sensorObj is LevelEntity && !LevelEditorCursor.IsCurrentlyMovingObject())
                {
                    var entity = (LevelEntity)sensorObj;

                    if (entity.resourceName == "Ship Pads/Ship Sensor Pad")
                    {
                        sensorEntity = entity;
                        tutorialStage++;
                    }
                }
                break;
            case 5:
                if(doorEntity == null)
                {
                    tutorialStage = 3;
                    return;
                }
                if (sensorEntity == null)
                {
                    tutorialStage = 4;
                    return;
                }

                if (sensorEntity.logicTarget == doorEntity) tutorialStage++;
                break;
        }
    }

    public void MoveToNextStage()
    {
        tutorialStage++;

        nextStageButton.interactable = false;
    }

    private void Update()
    {
        if (enableTutorial)
        {
            if (tutorialStage == 1)
            {
                Vector2 realLandPadPos = theRealLandPad.transform.position;
                Vector2 haloLandPadPos = landHalo.transform.position;

                followWorktablePosition = Vector2.Lerp(realLandPadPos, haloLandPadPos, 0.5f);
                followWorktablePosition.y = Mathf.Min(realLandPadPos.y, haloLandPadPos.y) - 1;
            }

            if (shouldFollowWorktablePosition)
            {
                rectTransform.position = Vector2.Lerp(rectTransform.position, MainCameraController.Singletron.selfCamera.WorldToScreenPoint(followWorktablePosition), 0.3f);
            }
            else
            {
                rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, new Vector2(0, 239.9f), 0.2f);
            }

            if (lastTutorialStage != tutorialStage)
            {
                UpdateTutorialText();
            }
            lastTutorialStage = tutorialStage;
        }
        else
        {
            HideTutorialText();
        }
    }
}
