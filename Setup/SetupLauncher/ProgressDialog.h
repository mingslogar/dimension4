#pragma once
#include "resource.h"
#include <afxcmn.h>

// ProgressDialog dialog

class ProgressDialog : public CDialog
{
	DECLARE_DYNAMIC(ProgressDialog)

public:
	ProgressDialog(CWnd* pParent = NULL);   // standard constructor
	virtual ~ProgressDialog();

// Dialog Data
	enum { IDD = IDD_DIALOG1 };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

	DECLARE_MESSAGE_MAP()
public:
	// ProgressDialog progress bar
	CProgressCtrl ProgressBar;
};
