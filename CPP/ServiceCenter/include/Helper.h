#ifndef HELPER_H
#define HELPER_H
#ifdef _WIN32
#include <stdio.h>
#include <Windows.h>
#include <iostream>
#include "psapi.h"
#include <tlhelp32.h>
 

HANDLE  getProcById(int pid = 0)
{
    HANDLE hProcess = 0 ;
    if(pid != 0)
      hProcess =  OpenProcess(PROCESS_ALL_ACCESS, false, pid);
      else 
       hProcess = GetCurrentProcess();
    return hProcess ;
}

double getCurrentMem(int pid)
{
    PROCESS_MEMORY_COUNTERS_EX pmc;
    HANDLE hProcess = getProcById(pid) ;
    GetProcessMemoryInfo(hProcess, (PROCESS_MEMORY_COUNTERS *)&pmc, sizeof(pmc));
    SIZE_T virtualMemUsedByMe = pmc.PrivateUsage;
    return virtualMemUsedByMe;
}
void begineGetCurrentCPUUsage(HANDLE & hProc,int & numProcessors,ULARGE_INTEGER& lastCPU, ULARGE_INTEGER & lastSysCPU,ULARGE_INTEGER& lastUserCPU){
    SYSTEM_INFO sysInfo;
    FILETIME ftime, fsys, fuser;

    GetSystemInfo(&sysInfo);
    numProcessors = sysInfo.dwNumberOfProcessors;

    GetSystemTimeAsFileTime(&ftime);
    memcpy(&lastCPU, &ftime, sizeof(FILETIME));
 
    GetProcessTimes(hProc, &ftime, &ftime, &fsys, &fuser);
    memcpy(&lastSysCPU, &fsys, sizeof(FILETIME));
    memcpy(&lastUserCPU, &fuser, sizeof(FILETIME));
}

double endGetCurrentCPUUsage(HANDLE & hProc,int & numProcessors,ULARGE_INTEGER& lastCPU, ULARGE_INTEGER & lastSysCPU,ULARGE_INTEGER& lastUserCPU){
    FILETIME ftime, fsys, fuser;
    ULARGE_INTEGER now, sys, user;
    double percent;

    GetSystemTimeAsFileTime(&ftime);
    memcpy(&now, &ftime, sizeof(FILETIME));

    GetProcessTimes(hProc, &ftime, &ftime, &fsys, &fuser);
    memcpy(&sys, &fsys, sizeof(FILETIME));
    memcpy(&user, &fuser, sizeof(FILETIME));
    percent = (sys.QuadPart - lastSysCPU.QuadPart) +
        (user.QuadPart - lastUserCPU.QuadPart);
    percent /= (now.QuadPart - lastCPU.QuadPart);
    percent /= numProcessors;
    lastCPU = now;
    lastUserCPU = user;
    lastSysCPU = sys;
    return percent * 100;
}

bool  HasProcessByName(const char * szProcName)
{
    bool has = false ;
    PROCESSENTRY32 entry;
    entry.dwSize = sizeof(PROCESSENTRY32);

    HANDLE snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, NULL);

    if (Process32First(snapshot, &entry) == TRUE)
    {
        while (Process32Next(snapshot, &entry) == TRUE)
        {
            if (stricmp(entry.szExeFile,szProcName) == 0)
            {  

                has = true ;
                break ;
            }
        }
    }

    CloseHandle(snapshot);
    return has ;
}

bool KillProcessByName(const char * szProcName)
{
     PROCESSENTRY32 entry;
    entry.dwSize = sizeof(PROCESSENTRY32);

    HANDLE snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, NULL);

    if (Process32First(snapshot, &entry) == TRUE)
    {
        while (Process32Next(snapshot, &entry) == TRUE)
        {
            if (stricmp(entry.szExeFile,szProcName) == 0)
            {  

               
                  HANDLE hProcess = OpenProcess(PROCESS_ALL_ACCESS, FALSE, entry.th32ProcessID);

                  TerminateProcess(hProcess,-1) ;

                  CloseHandle(hProcess);
                  break;
            }
        }
    }

    CloseHandle(snapshot);
    return true  ;
}

#elif __linux__
#include "stdlib.h"
#include "stdio.h"
#include "string.h"
#include "sys/times.h"
int parseLine(char* line){
    // This assumes that a digit will be found and the line ends in " Kb".
    int i = strlen(line);
    const char* p = line;
    while (*p <'0' || *p > '9') p++;
    line[i-3] = '\0';
    i = atoi(p);
    return i;
}

int getCurrentMem(){ //Note: this value is in KB!
    FILE* file = fopen("/proc/self/status", "r");
    int result = -1;
    char line[128];

    while (fgets(line, 128, file) != NULL){
        if (strncmp(line, "VmSize:", 7) == 0){
            result = parseLine(line);
            break;
        }
    }
    fclose(file);
    return result;
}


static clock_t lastCPU, lastSysCPU, lastUserCPU;
static int numProcessors;

void prepareMeasureCPUUsage(){
    FILE* file;
    struct tms timeSample;
    char line[128];

    lastCPU = times(&timeSample);
    lastSysCPU = timeSample.tms_stime;
    lastUserCPU = timeSample.tms_utime;

    file = fopen("/proc/cpuinfo", "r");
    numProcessors = 0;
    while(fgets(line, 128, file) != NULL){
        if (strncmp(line, "processor", 9) == 0) numProcessors++;
    }
    fclose(file);
}

double getCurrentCPUUsage(){
    struct tms timeSample;
    clock_t now;
    double percent;

    now = times(&timeSample);
    if (now <= lastCPU || timeSample.tms_stime < lastSysCPU ||
        timeSample.tms_utime < lastUserCPU){
        //Overflow detection. Just skip this value.
        percent = -1.0;
    }
    else{
        percent = (timeSample.tms_stime - lastSysCPU) +
            (timeSample.tms_utime - lastUserCPU);
        percent /= (now - lastCPU);
        percent /= numProcessors;
        percent *= 100;
    }
    lastCPU = now;
    lastSysCPU = timeSample.tms_stime;
    lastUserCPU = timeSample.tms_utime;

    return percent;
}


#endif
#endif