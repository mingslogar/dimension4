// ProgressDialog.cpp : implementation file
//

#include "stdafx.h"
#include "ProgressDialog.h"
//#include "afxdialogex.h"


// ProgressDialog dialog

IMPLEMENT_DYNAMIC(ProgressDialog, CDialog)

ProgressDialog::ProgressDialog(CWnd* pParent /*=NULL*/)
	: CDialog(ProgressDialog::IDD, pParent)
{
	Create(IDD);
	ProgressBar.SetStep(20);
}

ProgressDialog::~ProgressDialog()
{
}

void ProgressDialog::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_PROGRESS1, ProgressBar);
}


BEGIN_MESSAGE_MAP(ProgressDialog, CDialog)
//	ON_NOTIFY(NM_CUSTOMDRAW, IDC_PROGRESS1, &ProgressDialog::OnNMCustomdrawProgress1)
END_MESSAGE_MAP()