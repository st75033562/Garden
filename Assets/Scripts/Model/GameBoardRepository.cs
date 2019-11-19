using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameboardProject : Project
{
    private Gameboard.Gameboard m_gameboard = new Gameboard.Gameboard();

    public GameboardProject() { }

    public GameboardProject(Gameboard.Gameboard gameboard)
    {
        if (gameboard == null)
        {
            throw new ArgumentNullException("gameboard");
        }
        this.gameboard = gameboard;
    }

    public override string name
    {
        get
        {
            return base.name = m_gameboard.name;
        }
        set
        {
            base.name = value;
            m_gameboard.name = value;
        }
    }

    public Gameboard.Gameboard gameboard
    {
        get { return m_gameboard; }
        set
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            m_gameboard = value;
        }
    }

    public override IEnumerator<FileData> GetEnumerator()
    {
        for (var it = base.GetEnumerator(); it.MoveNext();)
        {
            yield return it.Current;
        }
        if (gameboard != null)
        {
            yield return new FileData(GameboardRepository.GameBoardFileName, gameboard.Serialize().ToByteArray());
        }
    }
}

public class GameboardRepository : CodeProjectRepository
{
    public const string GameBoardFileName = "gameboard.pro";

    public static GameboardRepository instance { get; set; }

    public GameboardRepository(VoiceRepository voiceRepo)
        : base(voiceRepo)
    {
    }

    public Gameboard.Gameboard getGameboard(string path)
    {
        byte[] data = loadFile(path, GameBoardFileName);
        if (data != null)
        {
            try
            {
                var gb = Gameboard.Gameboard.Parse(data);
                gb.name = Path.GetFileName(path);
                return gb;
            }
            catch (InvalidProtocolBufferException)
            {
                Debug.LogError("error parsing gameboard " + path);
            }
        }
        return null;
    }

    public bool saveGameboard(Gameboard.Gameboard gameboard)
    {
        return saveFile(gameboard.name, GameBoardFileName, gameboard.Serialize().ToByteArray());
    }

    public GameboardProject loadGameboardProject(string path)
    {
        var gameboard = getGameboard(path);
        if (gameboard == null)
        {
            return null;
        }
        var proj = new GameboardProject(gameboard);
        load(proj, path);
        return proj;
    }
}
