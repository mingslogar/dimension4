#pragma once
#include "resource.h"
#include <afxcmn.h>


// NETerror dialog

class NETerror : public CDialog
{
	DECLARE_DYNAMIC(NETerror)

public:
	NETerror(CWnd* pParent = NULL);   // standard constructor
	virtual ~NETerror();

// Dialog Data
	enum { IDD = IDD_DIALOG2 };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

	DECLARE_MESSAGE_MAP()
public:
	afx_msg void OnNMClickSyslink1(NMHDR *pNMHDR, LRESULT *pResult);
};
