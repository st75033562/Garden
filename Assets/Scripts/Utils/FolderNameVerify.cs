using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FolderNameVerify {

    public static string Verify(string value) {
        if (value == null)
        {
            throw new ArgumentNullException("value");
        }

        value = value.TrimEnd();
        if (value.Trim() == string.Empty)
        {
            return "name_white_space_only".Localize();
        }

        if (FileUtils.fileNameContainsInvalidChars(value))
        {
            return "file_name_invalid_char".Localize();
        }
        if (FileUtils.isReservedFileName(value))
        {
            return "file_name_reserved".Localize();
        }
        if (value.Length > ProjectRepository.MaxFileNameLength)
        {
            return "file_name_too_long".Localize(ProjectRepository.MaxFileNameLength);
        }
        if (value.StartsWith(CodeProjectRepository.FolderPrefix))
        {
            return "path_invalid_prefix".Localize(); ;
        }
        return null;
    }
}
